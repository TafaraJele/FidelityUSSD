using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd", IsNullable = false)]
    [DataContract(Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd")]
    [System.ServiceModel.XmlSerializerFormat]
    public partial class DeclineRp
    {

        private DeclineRpResponse responseField;

        [DataMember]
        public DeclineRpResponse Response
        {
            get
            {
                return this.responseField;
            }
            set
            {
                this.responseField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd")]
    [DataContract(Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd")]
    [System.ServiceModel.XmlSerializerFormat]
    public partial class DeclineRpResponse
    {

        private string productField;

        private byte responseField;

        private string nextChallengeField;

        private decimal verField;

        /// <remarks/>
        [DataMember, System.Xml.Serialization.XmlAttributeAttribute()]
        public string Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [DataMember, System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Response
        {
            get
            {
                return this.responseField;
            }
            set
            {
                this.responseField = value;
            }
        }

        /// <remarks/>
        [DataMember, System.Xml.Serialization.XmlAttributeAttribute()]
        public string NextChallenge
        {
            get
            {
                return this.nextChallengeField;
            }
            set
            {
                this.nextChallengeField = value;
            }
        }

        /// <remarks/>
        [DataMember, System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Ver
        {
            get
            {
                return this.verField;
            }
            set
            {
                this.verField = value;
            }
        }
    }
}








    //[DataContract(Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd")]
    //public partial class DeclineRp
    //{
    //    private DeclineRpResponse responseField;

    //    [DataMember]
    //    public DeclineRpResponse Response
    //    {
    //        get
    //        {
    //            return this.responseField;
    //        }
    //        set
    //        {
    //            this.responseField = value;
    //        }
    //    }
    //}


    //[DataContract(Namespace = "http://schemas.compassplus.com/two/1.0/fimi.xsd")]
    //[XmlSerializerFormat]
    //public partial class DeclineRpResponse
    //{

    //    private string productField;

    //    private byte responseField;

    //    private string nextChallengeField;

    //    private decimal verField;

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute()]
    //    public string Product
    //    {
    //        get
    //        {
    //            return this.productField;
    //        }
    //        set
    //        {
    //            this.productField = value;
    //        }
    //    }

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute()]
    //    public byte Response
    //    {
    //        get
    //        {
    //            return this.responseField;
    //        }
    //        set
    //        {
    //            this.responseField = value;
    //        }
    //    }

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute()]
    //    public string NextChallenge
    //    {
    //        get
    //        {
    //            return this.nextChallengeField;
    //        }
    //        set
    //        {
    //            this.nextChallengeField = value;
    //        }
    //    }

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute()]
    //    public decimal Ver
    //    {
    //        get
    //        {
    //            return this.verField;
    //        }
    //        set
    //        {
    //            this.verField = value;
    //        }
    //    }

