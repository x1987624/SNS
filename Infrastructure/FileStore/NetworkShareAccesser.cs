//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2013-02-18</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2013-02-18" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Runtime.InteropServices;
using System;
using System.ComponentModel;
using System.Net;

namespace Tunynet.FileStore
{
    /// <summary>
    /// 网络共享访问连接器，用于FileStore连接CIFS/Samba/NAS
    /// </summary>
    public class NetworkShareAccesser
    {

        /// <summary>
        /// 完整的UNC路径
        /// </summary>
        private string uncName;

        /// <summary>
        /// 访问共享连接的用户名
        /// </summary>
        private string username;

        /// <summary>
        /// 访问共享连接的密码
        /// </summary>
        private string password;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="uncName">完整的UNC路径</param>
        /// <param name="username">访问共享连接的用户名</param>
        /// <param name="password">访问共享连接的密码</param>
        public NetworkShareAccesser(string uncName, string username, string password)
        {
            this.uncName = uncName;
            this.username = username;
            this.password = password;
        }


        /// <summary>
        /// 创建一个网络连接
        /// </summary>
        public void Connect()
        {
            var netResource = new NetResource
                        {
                            Scope = ResourceScope.GlobalNetwork,
                            ResourceType = ResourceType.Disk,
                            DisplayType = ResourceDisplayType.Share,
                            RemoteName = this.uncName.TrimEnd('\\')
                        };

            var result = WNetAddConnection2(netResource, password, username, 0);

            if (result != 0)
            {
                throw new Win32Exception(result);
            }
        }

        /// <summary>
        /// 释放一个网络连接
        /// </summary>
        public void Disconnect()
        {
            WNetCancelConnection2(this.uncName, 1, true);
        }


        #region 使用Win32 API中的WNetUseConnection实现，下面是定义


        /// <summary>
        ///The WNetAddConnection2 function makes a connection to a network resource. The function can redirect a local device to the network resource.
        /// </summary>
        /// <param name="netResource">A <see cref="NetResource"/> structure that specifies details of the proposed connection, such as information about the network resource, the local device, and the network resource provider.</param>
        /// <param name="password">The password to use when connecting to the network resource.</param>
        /// <param name="username">The username to use when connecting to the network resource.</param>
        /// <param name="flags">The flags. See http://msdn.microsoft.com/en-us/library/aa385413%28VS.85%29.html for more information.</param>
        /// <returns></returns>
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
                                                     string password,
                                                     string username,
                                                     int flags);

        /// <summary>
        /// The WNetCancelConnection2 function cancels an existing network connection. You can also call the function to remove remembered network connections that are not currently connected.
        /// </summary>
        /// <param name="name">Specifies the name of either the redirected local device or the remote network resource to disconnect from.</param>
        /// <param name="flags">Connection type. The following values are defined:
        /// 0: The system does not update information about the connection. If the connection was marked as persistent in the registry, the system continues to restore the connection at the next logon. If the connection was not marked as persistent, the function ignores the setting of the CONNECT_UPDATE_PROFILE flag.
        /// CONNECT_UPDATE_PROFILE: The system updates the user profile with the information that the connection is no longer a persistent one. The system will not restore this connection during subsequent logon operations. (Disconnecting resources using remote names has no effect on persistent connections.)
        /// </param>
        /// <param name="force">Specifies whether the disconnection should occur if there are open files or jobs on the connection. If this parameter is FALSE, the function fails if there are open files or jobs.</param>
        /// <returns></returns>
        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        #endregion
    }


    /// <summary>
    /// The net resource.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplayType DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    /// <summary>
    /// The resource scope.
    /// </summary>
    public enum ResourceScope
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    } ;

    /// <summary>
    /// The resource type.
    /// </summary>
    public enum ResourceType
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }

    /// <summary>
    /// The resource displaytype.
    /// </summary>
    public enum ResourceDisplayType
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }

}