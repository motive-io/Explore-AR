using Motive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Motive.Core.Models
{
    [Serializable]
    [DataContract]
    public class QRTokenModel
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "projectConfig")]
        public ConfigInfoModel ProjectConfig { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "orgSpace")]
        public string OrgSpace { get; set; }

        public QRTokenModel() { }


    }

    [Serializable]
    public class QRTokenSpaceModel : QRTokenModel
    {
        [DataMember(Name = "space")]
        public SpaceInfoModel Space;

        public QRTokenSpaceModel() { }

        //public QRTokenSpaceModel(QRToken qrToken) : base(qrToken)
        //{
        //    if (qrToken?.ProjectConfig?.Space != null)
        //    {
        //        var s = qrToken?.ProjectConfig?.Space;
        //        var spaceModel = new SpaceModel()
        //        {
        //            name = s.Name,
        //            title = s.Title,
        //            fullName = s.FullName,
        //        };

        //        Space = spaceModel;
        //    }
        //}


    }
}
