using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FaaMvaToSectorFile
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1/message")]
    [XmlRoot(Namespace = "http://www.aixm.aero/schema/5.1/message", IsNullable = false)]
    public class AIXMBasicMessage
    {
        [XmlElement(Namespace = "http://www.opengis.net/gml/3.2")]
        public description description { get; set; }

        [XmlElement(Namespace = "http://www.opengis.net/gml/3.2", IsNullable = true)]
        public object boundedBy { get; set; }

        [XmlElement(Namespace = "http://www.aixm.aero/schema/5.1")]
        public byte sequenceNumber { get; set; }

        [XmlElement(Namespace = "http://www.aixm.aero/schema/5.1")]
        public object messageMetadata { get; set; }

        [XmlElement("hasMember")]
        public AIXMBasicMessageHasMember[] hasMember { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.opengis.net/gml/3.2")]
        public string id { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    [XmlRoot(Namespace = "http://www.opengis.net/gml/3.2", IsNullable = false)]
    public class description
    {

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/1999/xlink")]
        public string type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1/message")]
    public class AIXMBasicMessageHasMember
    {
        [XmlElement(Namespace = "http://www.aixm.aero/schema/5.1")]
        public Airspace Airspace { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/1999/xlink")]
        public string type { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    [XmlRoot(Namespace = "http://www.aixm.aero/schema/5.1", IsNullable = false)]
    public class Airspace
    {
        [XmlElement(Namespace = "http://www.opengis.net/gml/3.2")]
        public description description { get; set; }

        [XmlElement(Namespace = "http://www.opengis.net/gml/3.2", IsNullable = true)]
        public object boundedBy { get; set; }

        public AirspaceTimeSlice timeSlice { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.opengis.net/gml/3.2")]
        public string id { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class AirspaceTimeSlice
    {
        [XmlElement("AirspaceTimeSlice")]
        public AirspaceTimeSliceAirspaceTimeSlice AirspaceTimeSlice1 { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class AirspaceTimeSliceAirspaceTimeSlice
    {
        [XmlElement(Namespace = "http://www.opengis.net/gml/3.2")]
        public validTime validTime { get; set; }

        public string interpretation { get; set; }

        public byte sequenceNumber { get; set; }

        public string type { get; set; }

        [XmlElement(IsNullable = true)]
        public object designator { get; set; }

        [XmlElement(IsNullable = true)]
        public object localType { get; set; }

        public string name { get; set; }

        [XmlElement(IsNullable = true)]
        public object designatorICAO { get; set; }

        [XmlElement(IsNullable = true)]
        public object controlType { get; set; }

        [XmlElement(IsNullable = true)]
        public object upperLowerSeparation { get; set; }

        [XmlElement(IsNullable = true)]
        public object protectedRoute { get; set; }

        public AirspaceTimeSliceAirspaceTimeSliceGeometryComponent geometryComponent { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    [XmlRoot(Namespace = "http://www.opengis.net/gml/3.2", IsNullable = false)]
    public class validTime
    {
        public validTimeTimePeriod TimePeriod { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/1999/xlink")]
        public string type { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class validTimeTimePeriod
    {
        public DateTime beginPosition { get; set; }

        public validTimeTimePeriodEndPosition endPosition { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class validTimeTimePeriodEndPosition
    {
        [XmlAttribute]
        public string indeterminatePosition { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class AirspaceTimeSliceAirspaceTimeSliceGeometryComponent
    {
        public AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponent AirspaceGeometryComponent
        {
            get;
            set;
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponent
    {
        [XmlElement(IsNullable = true)]
        public object operation { get; set; }

        [XmlElement(IsNullable = true)]
        public object operationSequence { get; set; }

        public AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolume
            theAirspaceVolume { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolume
    {
        public
            AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolume
            AirspaceVolume { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class
        AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolume
    {
        public
            AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolumeMinimumLimit
            minimumLimit { get; set; }

        public string minimumLimitReference { get; set; }

        public
            AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolumeHorizontalProjection
            horizontalProjection { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class
        AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolumeMinimumLimit
    {
        [XmlAttribute]
        public string uom { get; set; }


        [XmlText]
        public ushort Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class
        AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolumeHorizontalProjection
    {
        public
            AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolumeHorizontalProjectionSurface
            Surface { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.aixm.aero/schema/5.1")]
    public class
        AirspaceTimeSliceAirspaceTimeSliceGeometryComponentAirspaceGeometryComponentTheAirspaceVolumeAirspaceVolumeHorizontalProjectionSurface
    {
        [XmlElement(Namespace = "http://www.opengis.net/gml/3.2")]
        public patches patches { get; set; }

        [XmlAttribute]
        public string srsName { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.opengis.net/gml/3.2")]
        public string id { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    [XmlRoot(Namespace = "http://www.opengis.net/gml/3.2", IsNullable = false)]
    public class patches
    {
        public patchesPolygonPatch PolygonPatch { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class patchesPolygonPatch
    {
        public patchesPolygonPatchExterior exterior { get; set; }

        [XmlElement("interior")]
        public List<patchesPolygonPatchInterior> interior { get; set; }

        [XmlAttribute]
        public string interpolation { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class patchesPolygonPatchExterior
    {
        public patchesPolygonPatchExteriorLinearRing LinearRing { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class patchesPolygonPatchExteriorLinearRing
    {
        public string posList { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class patchesPolygonPatchInterior
    {
        public patchesPolygonPatchInteriorLinearRing LinearRing { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/gml/3.2")]
    public class patchesPolygonPatchInteriorLinearRing
    {
        public string posList { get; set; }
    }
}