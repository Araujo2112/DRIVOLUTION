# Factory


-  `id`: Unique identifier of the factory
   -  Attribute type: **Property**. 
   -  Required
-  `type`: Defines the type as 'Factory'. One of : `Factory`.
   -  Attribute type: **Property**. 
   -  Optional
-  `name`: Name of the factory
   -  Attribute type: **Property**. 
   -  Required
-  `location`: Geographical location of the factory
   -  Attribute type: **Property**. 
   -  Optional
-  `sectors`: Sectors associated with this factory
   -  Attribute type: **Property**. 
   -  Optional



# Sector


-  `id`: Unique identifier of the sector
   -  Attribute type: **Property**. 
   -  Required
-  `type`: Defines the type as 'Sector'. One of : `Sector`.
   -  Attribute type: **Property**. 
   -  Optional
-  `factoryId`: Relationship with the factory it belongs to
   -  Attribute type: **Property**. 
   -  Optional
-  `location`: Geographical location of the sector
   -  Attribute type: **Property**. 
   -  Optional
-  `containers`: Containers associated with this sector
   -  Attribute type: **Property**. 
   -  Optional
-  `porticos`: Porticos associated with this sector
   -  Attribute type: **Property**. 
   -  Optional



# Portico


-  `id`: Unique identifier of the portico
   -  Attribute type: **Property**. 
   -  Required
-  `type`: Defines the type as 'Portico'. One of : `Portico`.
   -  Attribute type: **Property**. 
   -  Optional
-  `location`: Geographical location of the portico
   -  Attribute type: **Property**. 
   -  Optional
-  `sensors`: Sensors associated with this portico
   -  Attribute type: **Property**. 
   -  Optional



# FillingLevelSensor

A device that consists of a sensor, has the category `saref:Sensor`, and is used to detect the filling level.

-  `batteryLevel`: Device's battery level. It must be equal to `1.0` when battery is full. `0.0` when battery ìs empty. `-1` when transiently cannot be determined.
-  Allowed values: Interval \[0,1\] and -1
   -  Attribute type: **Property**. [Number](https://schema.org/Number)
   -  Optional
-  `category`: See attribute `category` from [DeviceModel](../../DeviceModel/doc/spec.md). Optional but recommended to optimize queries.
   -  Attribute type: **EnumProperty**. 
   -  Required
-  `configuration`: Device's technical configuration. This attribute is intended to be a dictionary of properties which capture parameters which have to do with the configuration of a device (timeouts, reporting periods, etc.) and which are not currently covered by the standard attributes defined by this model.
   -  Attribute type: **Property**. [StructuredValue](https://schema.org/StructuredValue)
   -  Optional
   -  Meta Data: 
       -  `dateModified`: Last update timestamp of this attribute
           -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
-  `controlledAsset`: The asset(s) (building, object, etc.) controlled by the device.
   -  Attribute type: **Relationship**. [URL](https://schema.org/URL)
   -  Optional
-  `controlledProperty`: See attribute `controlledProperty` from [DeviceModel](../../DeviceModel/doc/spec.md). Optional but recommended to optimize queries.
   -  Attribute type: **EnumProperty**. 
   -  Optional
-  `dataProvider`: Specifies the URL to information about the provider of this information
   -  Attribute type: **Property**. [URL](https://schema.org/URL)
   -  Optional
-  `dateFirstUsed`: A timestamp which denotes when the device was first used.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateInstalled`: A timestamp which denotes when the device was installed
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateLastCalibration`: A timestamp which denotes when the last calibration of the device happened.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateLastValueReported`: A timestamp which denotes the last time when the
 device successfully reported data to the cloud.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateManufactured`: A timestamp which denotes when the device was manufactured.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `description`: A description of the item
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
   -  Normative References: http://purl.org/dc/elements/1.1/description
-  `deviceState`: State of this device from an operational point of view. Its
 value can be vendor dependent.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `firmwareVersion`: The firmware version of this device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `hardwareVersion`: The hardware version of this device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `ipAddress`: The IP address of the device. It can be a comma separated list of values if the device has more than one IP address.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `location`: The current location of the item
   -  Attribute type: **GeoProperty**. [Point](https://purl.org/geojson/vocab#Point) or [LineString](https://purl.org/geojson/vocab#LineString) or [Polygon](https://purl.org/geojson/vocab#Polygon) or [MultiPoint](https://purl.org/geojson/vocab#MultiPoint) or [MultiLineString](https://purl.org/geojson/vocab#MultiLineString) or [MultiPolygon](https://purl.org/geojson/vocab#MultiPolygon)
   -  Optional
   -  Normative References: http://geojson.org/geojson-spec.html#geometry-objects
-  `macAddress`: The MAC address of the device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `mcc`: Mobile Country Code - This property identifies univoquely the country of the mobile network the device is attached to.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `mnc`: This property identifies the Mobile Network Code (MNC) of the network the device is attached to. The MNC is used in combination with a Mobile Country Code (MCC) (also known as a "MCC / MNC tuple") to uniquely identify a mobile phone operator/carrier using the GSM, CDMA, iDEN, TETRA and 3G / 4G public land mobile networks and some satellite mobile
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `name`: A mnemonic name given to the device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `osVersion`: The version of the host operating system device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `owner`: The owners of a Device.
   -  Attribute type: **Relationship**. [Person](http://schema.org/Person) or [Organization](https://schema.org/Organization)
   -  Optional
-  `provider`: The provider of the device.
   -  Attribute type: **Property**. [provider](https://schema.org/provider)
   -  Optional
-  `refDeviceModel`: The device's model.
   -  Attribute type: **Relationship**. [DeviceModel](https://uri.fiware.org/ns/data-models#DeviceModel)
   -  Optional
-  `rssi`: Received signal strength indicator for a wireless enabled device. It must be equal to `1.0` when the signal strength is maximum. `0.0` when signal is missing. `-1.0` when it cannot be determined.
-  Allowed values: Interval \[0,1\] and -1
   -  Attribute type: **Property**. [Number](https://schema.org/Number)
   -  Optional
-  `serialNumber`: The serial number assigned by the manufacturer. see [https://schema.org/serialNumber](https://schema.org/serialNumber)
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `softwareVersion`: The software version of this device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `source`: A sequence of characters giving the source of the entity data.
   -  Attribute type: **Property**. [Text](https://schema.org/Text) or [URL](https://schema.org/URL)
   -  Optional
-  `supportedProtocol`: See attribute `supportedProtocol` from [DeviceModel](../../DeviceModel/doc/spec.md). Needed if due to a software update new protocols are supported. Otherwise it is better to convey it at `DeviceModel` level.
   -  Attribute type: **EnumProperty**. 
   -  Optional
-  `value`: A observed or reported value. For actuator devices, it is an attribute that allows a controlling application to change the actuation setting. For instance, a switch device which is currently _on_ can report a value `"on"`of type `Text`. Obviously, in order to toggle the referred switch, this attribute value will have to be changed to `"off"`.
   -  Attribute type: **Property**. [Text](https://schema.org/Text) or [QuantitativeValue](https://schema.org/QuantitativeValue)
   -  Optional
-  `id`: Unique identifier of the filling level sensor
   -  Attribute type: **Property**. 
   -  Required
-  `fillingLevel`: Property related to some measurements that are characterized by a certain value that is a filling level.
   -  Attribute type: **Property**. [Number](https://schema.org/Number)
   -  Optional
   -  Meta Data: 
       -  `providedBy`: The device that sent this reading
           -  Attribute type: **Relationship**. [URL](https://schema.org/URL)
       -  `observedAt`: A timestamp which denotes when the reading was taken
           -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
       -  `unitCode`: A string representing the measurement unit corresponding to the Property value. It shall be encoded using the UN/CEFACT Common Codes for Units of Measurement
           -  Attribute type: **Property**. [Text](https://schema.org/Text)



# MotionSensor

A device that consists of a motion sensor, has the category `saref:Sensor`, and is used to detect motion.

-  `batteryLevel`: Device's battery level. It must be equal to `1.0` when battery is full. `0.0` when battery ìs empty. `-1` when transiently cannot be determined.
-  Allowed values: Interval \[0,1\] and -1
   -  Attribute type: **Property**. [Number](https://schema.org/Number)
   -  Optional
-  `category`: See attribute `category` from [DeviceModel](../../DeviceModel/doc/spec.md). Optional but recommended to optimize queries.
   -  Attribute type: **EnumProperty**. 
   -  Required
-  `configuration`: Device's technical configuration. This attribute is intended to be a dictionary of properties which capture parameters which have to do with the configuration of a device (timeouts, reporting periods, etc.) and which are not currently covered by the standard attributes defined by this model.
   -  Attribute type: **Property**. [StructuredValue](https://schema.org/StructuredValue)
   -  Optional
   -  Meta Data: 
       -  `dateModified`: Last update timestamp of this attribute
           -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
-  `controlledAsset`: The asset(s) (building, object, etc.) controlled by the device.
   -  Attribute type: **Relationship**. [URL](https://schema.org/URL)
   -  Optional
-  `controlledProperty`: See attribute `controlledProperty` from [DeviceModel](../../DeviceModel/doc/spec.md). Optional but recommended to optimize queries.
   -  Attribute type: **EnumProperty**. 
   -  Optional
-  `dataProvider`: Specifies the URL to information about the provider of this information
   -  Attribute type: **Property**. [URL](https://schema.org/URL)
   -  Optional
-  `dateFirstUsed`: A timestamp which denotes when the device was first used.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateInstalled`: A timestamp which denotes when the device was installed
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateLastCalibration`: A timestamp which denotes when the last calibration of the device happened.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateLastValueReported`: A timestamp which denotes the last time when the
 device successfully reported data to the cloud.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `dateManufactured`: A timestamp which denotes when the device was manufactured.
   -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
   -  Optional
-  `description`: A description of the item
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
   -  Normative References: http://purl.org/dc/elements/1.1/description
-  `deviceState`: State of this device from an operational point of view. Its
 value can be vendor dependent.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `firmwareVersion`: The firmware version of this device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `hardwareVersion`: The hardware version of this device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `ipAddress`: The IP address of the device. It can be a comma separated list of values if the device has more than one IP address.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `location`: The current location of the item
   -  Attribute type: **GeoProperty**. [Point](https://purl.org/geojson/vocab#Point) or [LineString](https://purl.org/geojson/vocab#LineString) or [Polygon](https://purl.org/geojson/vocab#Polygon) or [MultiPoint](https://purl.org/geojson/vocab#MultiPoint) or [MultiLineString](https://purl.org/geojson/vocab#MultiLineString) or [MultiPolygon](https://purl.org/geojson/vocab#MultiPolygon)
   -  Optional
   -  Normative References: http://geojson.org/geojson-spec.html#geometry-objects
-  `macAddress`: The MAC address of the device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `mcc`: Mobile Country Code - This property identifies univoquely the country of the mobile network the device is attached to.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `mnc`: This property identifies the Mobile Network Code (MNC) of the network the device is attached to. The MNC is used in combination with a Mobile Country Code (MCC) (also known as a "MCC / MNC tuple") to uniquely identify a mobile phone operator/carrier using the GSM, CDMA, iDEN, TETRA and 3G / 4G public land mobile networks and some satellite mobile
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `name`: A mnemonic name given to the device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `osVersion`: The version of the host operating system device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `owner`: The owners of a Device.
   -  Attribute type: **Relationship**. [Person](http://schema.org/Person) or [Organization](https://schema.org/Organization)
   -  Optional
-  `provider`: The provider of the device.
   -  Attribute type: **Property**. [provider](https://schema.org/provider)
   -  Optional
-  `refDeviceModel`: The device's model.
   -  Attribute type: **Relationship**. [DeviceModel](https://uri.fiware.org/ns/data-models#DeviceModel)
   -  Optional
-  `rssi`: Received signal strength indicator for a wireless enabled device. It must be equal to `1.0` when the signal strength is maximum. `0.0` when signal is missing. `-1.0` when it cannot be determined.
-  Allowed values: Interval \[0,1\] and -1
   -  Attribute type: **Property**. [Number](https://schema.org/Number)
   -  Optional
-  `serialNumber`: The serial number assigned by the manufacturer. see [https://schema.org/serialNumber](https://schema.org/serialNumber)
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `softwareVersion`: The software version of this device.
   -  Attribute type: **Property**. [Text](https://schema.org/Text)
   -  Optional
-  `source`: A sequence of characters giving the source of the entity data.
   -  Attribute type: **Property**. [Text](https://schema.org/Text) or [URL](https://schema.org/URL)
   -  Optional
-  `supportedProtocol`: See attribute `supportedProtocol` from [DeviceModel](../../DeviceModel/doc/spec.md). Needed if due to a software update new protocols are supported. Otherwise it is better to convey it at `DeviceModel` level.
   -  Attribute type: **EnumProperty**. 
   -  Optional
-  `value`: A observed or reported value. For actuator devices, it is an attribute that allows a controlling application to change the actuation setting. For instance, a switch device which is currently _on_ can report a value `"on"`of type `Text`. Obviously, in order to toggle the referred switch, this attribute value will have to be changed to `"off"`.
   -  Attribute type: **Property**. [Text](https://schema.org/Text) or [QuantitativeValue](https://schema.org/QuantitativeValue)
   -  Optional
-  `id`: Unique identifier of the motion sensor
   -  Attribute type: **Property**. 
   -  Required
-  `motion`: Property related to some measurements that are characterized by a certain value that is measured in a unit of measure for motion
   -  Attribute type: **Property**. [Number](https://schema.org/Number)
   -  Optional
   -  Meta Data: 
       -  `providedBy`: The device that sent this reading
           -  Attribute type: **Relationship**. [URL](https://schema.org/URL)
       -  `observedAt`: A timestamp which denotes when the reading was taken
           -  Attribute type: **Property**. [DateTime](https://schema.org/DateTime)
       -  `unitCode`: A string representing the measurement unit corresponding to the Property value. It shall be encoded using the UN/CEFACT Common Codes for Units of Measurement
           -  Attribute type: **Property**. [Text](https://schema.org/Text)



# Container


-  `id`: Unique identifier of the container
   -  Attribute type: **Property**. 
   -  Required
-  `type`: Defines the type as 'Container'. One of : `Container`.
   -  Attribute type: **Property**. 
   -  Optional
-  `container_code`: Container code
   -  Attribute type: **Property**. 
   -  Optional
-  `container_volume`: Container volume in liters
   -  Attribute type: **Property**. 
   -  Optional
-  `sectorId`: Relationship with the sector to which the container belongs
   -  Attribute type: **Property**. 
   -  Optional
-  `active`: Indicates if the container is active
   -  Attribute type: **Property**. 
   -  Optional



## Examples

### List of entities


