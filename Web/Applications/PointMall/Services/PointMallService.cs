//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.PointMall
{

    /// <summary>
    /// 商城业务逻辑类
    /// </summary>
    public class PointMallService
    {
        private IPointGiftRepository giftRepository;
        private IPointGiftExchangeRecordRepository recordRepository;
        private IMailAddressRepository mailAddressRepository;
        private AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().PointGift());
        private PointService pointService = new PointService();
        private UserService userService = DIContainer.Resolve<UserService>();

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public PointMallService()
            : this(new PointGiftRepository(), new PointGiftExchangeRecordRepository(), new MailAddressRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="giftRepository">商品仓储实现</param>
        /// <param name="recordRepository">兑换记录仓储实现</param>
        /// <param name="mailAddressRepository">邮寄信息仓储实现</param>
        public PointMallService(IPointGiftRepository giftRepository, IPointGiftExchangeRecordRepository recordRepository, IMailAddressRepository mailAddressRepository)
        {
            this.giftRepository = giftRepository;
            this.recordRepository = recordRepository;
            this.mailAddressRepository = mailAddressRepository;
        }
        #endregion    
   
        #region 商品维护
        /// <summary>
        /// 创建商品
        /// </summary>
        /// <remarks>
        /// 1.创建商品
        /// 2.处理商品图片，将附件关联到商品
        /// </remarks>
        /// <param name="gift">商品对象</param>
        /// <returns></returns>
        public bool CreateGift(PointGift gift)
        {
            giftRepository.Insert(gift);
            
            //处理展示图            
            attachmentService.ToggleTemporaryAttachments(gift.UserId, TenantTypeIds.Instance().PointGift(), gift.GiftId);

            return gift.GiftId > 0;
        }

        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <remarks>
        /// 1.编辑商品
        /// 2.处理商品图片，将附件关联到商品
        /// </remarks>
        /// <param name="gift">商品对象</param>
        public void UpdateGift(PointGift gift)
        {
            giftRepository.Update(gift);

            //处理展示图
            attachmentService.ToggleTemporaryAttachments(gift.UserId, TenantTypeIds.Instance().PointGift(), gift.GiftId);
        }        

        /// <summary>
        /// 是否上架
        /// </summary>
        /// <param name="gift">商品对象</param>
        /// <param name="isEnabled">是否显示</param>
        public void SetEnabled(PointGift gift, bool isEnabled)
        {
            if (gift.IsEnabled != isEnabled)
            {
                gift.IsEnabled = isEnabled;
                gift.LastModified = DateTime.UtcNow;
                giftRepository.Update(gift);
            }
        }
        #endregion

        #region 商品查询
        /// <summary>
        /// 获取商品对象
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <returns>返回商品对象</returns>
        public PointGift GetGift(long giftId)
        {
            return giftRepository.Get(giftId);
        }

        /// <summary>
        /// 获取商品列表（根据Id组装实体）
        /// </summary>
        /// <param name="giftIds">商品ID列表</param>
        /// <returns>返回商品列表</returns>
        public IEnumerable<PointGift> GetGifts(IEnumerable<long> giftIds)
        {
            return giftRepository.PopulateEntitiesByEntityIds(giftIds);
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="nameKeyword">商品名称关键字</param>
        /// <param name="categoryId">类别ID</param>
        /// <param name="sortBy">排序规则</param>
        /// <param name="maxPrice">最大单价</param>
        /// <param name="minPrice">最小单价</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGift> GetGifts(string nameKeyword, long? categoryId, SortBy_PointGift sortBy = SortBy_PointGift.DateCreated_Desc, int maxPrice = 0, int minPrice = 0, int pageSize = 20, int pageIndex = 1)
        {
            return giftRepository.GetPointGifts(nameKeyword, categoryId, sortBy, maxPrice, minPrice, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>       
        /// <param name="sortby">排序规则</param>
        /// <param name="topNumber">条数</param>
        /// <returns></returns>
        public IEnumerable<PointGift> GetGifts(SortBy_PointGift sortby,int topNumber)
        {
            return giftRepository.GetPointGifts(sortby, topNumber);
        }

        /// <summary>
        /// 管理员获取商品数据
        /// </summary>
        /// <param name="nameKeyword">商品名称关键字</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="maxPrice">最大单价</param>
        /// <param name="minPrice">最小单价</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGift> GetGiftsForAdmin(string nameKeyword,long? categoryId,bool? isEnabled,int maxPrice = 0, int minPrice = 0, int pageSize = 20, int pageIndex = 1)
        {
            return giftRepository.GetGiftsForAdmin(nameKeyword, categoryId,isEnabled, maxPrice, minPrice, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取商品兑换次数
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <returns></returns>
        public int GetGiftExchangeNumber(long giftId)
        {
            return giftRepository.GetGiftExchangeNumber(giftId);
        }
        #endregion

        #region 兑换记录维护
        /// <summary>
        /// 创建兑换记录
        /// </summary>
        /// <remarks>
        /// 1.冻结相应积分
        /// </remarks>
        /// <param name="record">记录对象</param>
        /// <returns></returns>
        public long CreateRecord(PointGiftExchangeRecord record)
        {
            if (record.Price > 0)
            {
                userService.FreezeTradePoints(record.PayerUserId, record.Price);

                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(record.PayerUserId, OwnerDataKeys.Instance().PostCount(), 1);   
            }
            recordRepository.Insert(record);
            return record.RecordId;
        }

        /// <summary>
        /// 是否批准申请
        /// </summary>
        /// <remarks>
        /// 1.需要触发的事件:1)Update的OnBefore、OnAfter；
        /// 2.如果申请通过，则不能被取消。
        /// 3.申请通过后触发动态，通知。
        /// 4.申请被否决后，积分返还。
        /// </remarks>
        /// <param name="record">记录对象</param>
        /// <param name="isApprove">是否批准</param>
        public void IsApprove(PointGiftExchangeRecord record, bool isApprove)
        {           
            if (isApprove)
            {
                PointGift gift = this.GetGift(record.GiftId);
                record.LastModified = DateTime.UtcNow;
                record.Status = ApproveStatus.Approved;
                gift.ExchangedCount = gift.ExchangedCount + record.Number;
                giftRepository.Update(gift);
                pointService.TradeToSystem(record.PayerUserId, record.Price, "兑换商品", false);
                EventBus<PointGiftExchangeRecord>.Instance().OnAfter(record, new CommonEventArgs(EventOperationType.Instance().Approved()));
            }
            else
            {
                record.Status = ApproveStatus.Rejected;
                userService.UnfreezeTradePoints(record.PayerUserId, record.Price);
                EventBus<PointGiftExchangeRecord>.Instance().OnAfter(record, new CommonEventArgs(EventOperationType.Instance().Disapproved()));
            }

            //处理统计数据
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(record.PayerUserId, OwnerDataKeys.Instance().PostCount(), -1);

            recordRepository.Update(record);                      
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <remarks>
        /// 1.管理员无权删除兑换记录
        /// 2.记录已经批准则不能删除记录
        /// 3.冻结积分返还
        /// </remarks>
        /// <param name="record">记录对象</param>
        public bool CancelRecord(PointGiftExchangeRecord record)
        {
            if (record.Status == ApproveStatus.Pending)
            {
                userService.UnfreezeTradePoints(record.PayerUserId, record.Price);

                //处理统计数据
                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(record.PayerUserId, OwnerDataKeys.Instance().PostCount(), -1);

                recordRepository.Delete(record);
                return true;
            }
            else
            {
                return false;
            }       
        }

        /// <summary>
        /// 设置评价
        /// </summary>
        /// <param name="recordId">记录ID</param>
        /// <param name="appraise">评价内容</param>
        public void SetAppraise(long recordId, string appraise)
        {
            PointGiftExchangeRecord record =this.GetRecord(recordId);
            record.Appraise=appraise;
            recordRepository.Update(record);
        }

        /// <summary>
        /// 设置跟踪信息
        /// </summary>
        /// <param name="recordId">记录ID</param>
        /// <param name="trackInfo">跟踪信息</param>
        public void SetTrackInfo(long recordId, string trackInfo)
        {
            PointGiftExchangeRecord record = this.GetRecord(recordId);
            record.TrackInfo = trackInfo;
            recordRepository.Update(record);
        }

        /// <summary>
        /// 删除用户时处理其兑换申请
        /// </summary>
        /// <remarks>
        /// 1.如果需要接管，直接执行Repository的方法更新数据库记录
        /// 2.如果不需要接管，删除申请记录
        /// </remarks>
        /// <param name="userId">用户ID</param>
        /// <param name="takeOverUserName">指定接管用户的用户名</param>
        /// <param name="isTakeOver">是否接管</param>
        public void DeleteUser(long userId, string takeOverUserName, bool isTakeOver)
        {
            if (isTakeOver)
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                User takeOverUser = userService.GetFullUser(takeOverUserName);
                recordRepository.TakeOver(userId, takeOverUser);
            }
            else
            {
                //删除用户下申请
                int pageCount = 1;
                int pageIndex = 1;
                do
                {
                    PagingDataSet<PointGiftExchangeRecord> records = this.GetRecordsOfUser(userId, Convert.ToDateTime("2000-01-01"),DateTime.Now, null, 100, pageIndex);
                    foreach (PointGiftExchangeRecord record in records)
                    {
                        recordRepository.Delete(record);

                        //处理统计数据
                        OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                        ownerDataService.Change(record.PayerUserId, OwnerDataKeys.Instance().PostCount(), -1);
                    }
                    pageCount = records.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);

            }
        }
        #endregion

        #region 兑换记录查询
        /// <summary>
        /// 获取记录对象
        /// </summary>
        /// <param name="recordId">记录ID</param>
        /// <returns>返回记录对象</returns>
        public PointGiftExchangeRecord GetRecord(long recordId)
        {
            return recordRepository.Get(recordId);
        }

        /// <summary>
        /// 获取记录列表（根据Id组装实体）
        /// </summary>
        /// <param name="recordIds">记录ID列表</param>
        /// <returns>返回记录列表</returns>
        public IEnumerable<PointGiftExchangeRecord> GetRecords(IEnumerable<long> recordIds)
        {
            return recordRepository.PopulateEntitiesByEntityIds(recordIds);
        }

        /// <summary>
        /// 获取评价（用于频道）
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGiftExchangeRecord> GetRecords(long giftId, int pageSize = 20, int pageIndex = 1,ApproveStatus? approveStatus=null)
        {
            return recordRepository.GetRecords(giftId, pageSize, pageIndex,approveStatus);
        }

        public PagingDataSet<PointGiftExchangeRecord> GetRecordsCount(long giftId, ApproveStatus? approveStatus = null)
        {
            return recordRepository.GetRecordsCount(giftId,approveStatus);
        }

        /// <summary>
        /// 获取兑换记录(用于空间)
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="year">年份</param>
        /// <param name="approveStatus">是否批准</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGiftExchangeRecord> GetRecordsOfUser(long userId, DateTime beginDate, DateTime endDate, ApproveStatus? approveStatus, int pageSize = 20, int pageIndex = 1)
        {
            return recordRepository.GetRecordsOfUser(userId, beginDate, endDate, approveStatus, pageSize, pageIndex);
        }

        /// <summary>
        /// 管理员获取兑换记录
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="recordStatus">记录状态</param>
        /// <param name="beginDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGiftExchangeRecord> GetRecordsForAdmin(long? userId, ApproveStatus? recordStatus, DateTime? beginDate, DateTime endDate, int pageSize = 20, int pageIndex = 1)
        {
            return recordRepository.GetRecordsForAdmin(userId, recordStatus, beginDate, endDate, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取应用统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回应用统计数据</returns>
        public Dictionary<string, long> GetRecordApplicationStatisticData(string tenantTypeId = null)
        {
            return recordRepository.GetRecordApplicationStatisticData(tenantTypeId);
        }

        /// <summary>
        /// 获取申请管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回申请管理数据</returns>
        public Dictionary<string, long> GetRecordManageableData(string tenantTypeId = null)
        {
            return recordRepository.GetRecordManageableData(tenantTypeId);
        }
        #endregion          
    }
}