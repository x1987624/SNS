////<TunynetCopyright>
////--------------------------------------------------------------
////<copyright>tunynet inc. 2005-2013</copyright>
////<version>V0.5</verion>
////<createdate>2010-10-17</createdate>
////<author>mazq</author>
////<email>mazq@tunynet.com</email>
////<log date="2010-10-17" version="0.5">创建</log>
////--------------------------------------------------------------
////</TunynetCopyright>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Lucene.Net.Documents;

//namespace Spacebuilder.CMS.Search
//{
//    /// <summary>
//    /// Lucene搜索结果项
//    /// </summary>
//    public class LuceneSearchHit : ISearchHit
//    {
//        private readonly Document _doc;
//        private readonly float _score;

//        public LuceneSearchHit(Document document, float score)
//        {
//            _doc = document;
//            _score = score;
//        }

//        #region ISearchHit 成员

//        public int ContentItemId
//        {
//            get { return GetInt(ContentItemIndexFields.ContentItemId); }
//        }

//        public float Score { get { return _score; } }

//        public int GetInt(string fieldName)
//        {
//            int returnValue = 0;

//            string fieldValue = _doc.Get(fieldName);
//            if (fieldValue != null)
//                int.TryParse(fieldValue, out returnValue);

//            return returnValue;
//        }

//        public decimal GetDecimal(string fieldName)
//        {
//            decimal returnValue = 0;

//            string fieldValue = _doc.Get(fieldName);
//            if (fieldValue != null)
//                decimal.TryParse(fieldValue, out returnValue);

//            return returnValue;
//        }

//        public bool GetBoolean(string fieldName)
//        {
//            bool returnValue = false;

//            string fieldValue = _doc.Get(fieldName);
//            if (fieldValue != null)
//                bool.TryParse(fieldValue, out returnValue);

//            return returnValue;
//        }

//        public string GetString(string fieldName)
//        {
//            return _doc.Get(fieldName);
//        }

//        public DateTime GetDateTime(string fieldName)
//        {
//            DateTime returnValue = DateTime.MinValue;
//            string fieldValue = _doc.Get(fieldName);
//            if (fieldValue != null)
//            {
//                try
//                {
//                    returnValue = DateTools.StringToDate(fieldValue);
//                }
//                catch { }

//            }
//            return returnValue;
//        }

//        #endregion

//    }
//}
