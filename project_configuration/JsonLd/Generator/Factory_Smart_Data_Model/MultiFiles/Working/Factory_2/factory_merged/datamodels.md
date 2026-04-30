# Factory


-  `id`: Unique identifier for the factory.
   -  Attribute type: **Property**. 
   -  Required
-  `name`: Name of the factory.
   -  Attribute type: **Property**. 
   -  Required
-  `description`: Detailed description of the factory.
   -  Attribute type: **Property**. 
   -  Required
-  `location`: Physical location of the factory.
   -  Attribute type: **Property**. 
   -  Required
-  `typeIndustry`: The type of industry the factory operates in.
   -  Attribute type: **Property**. 
   -  Required
-  `numberOfEmployees`: The number of employees working at the factory.
   -  Attribute type: **Property**. 
   -  Required
-  `sectors`: List of sectors within the factory.
   -  Attribute type: **Property**. 
   -  Required



# Sector


-  `id`: Unique identifier for the sector.
   -  Attribute type: **Property**. 
   -  Required
-  `name`: Name of the sector.
   -  Attribute type: **Property**. 
   -  Required
-  `description`: Detailed description of the sector.
   -  Attribute type: **Property**. 
   -  Required
-  `checkpoints`: List of checkpoints within the sector.
   -  Attribute type: **Property**. 
   -  Required
-  `containers`: List of containers within the sector.
   -  Attribute type: **Property**. 
   -  Required



# Container


-  `id`: Unique identifier for the container.
   -  Attribute type: **Property**. 
   -  Required
-  `name`: Name of the container.
   -  Attribute type: **Property**. 
   -  Required
-  `description`: Detailed description of the container.
   -  Attribute type: **Property**. 
   -  Required
-  `capacity`: Capacity of the container (in cubic meters or appropriate unit).
   -  Attribute type: **Property**. 
   -  Required



# Checkpoint


-  `id`: Unique identifier for the checkpoint.
   -  Attribute type: **Property**. 
   -  Required
-  `name`: Name of the checkpoint.
   -  Attribute type: **Property**. 
   -  Required
-  `description`: Detailed description of the checkpoint, including its use and specifications.
   -  Attribute type: **Property**. 
   -  Required
-  `sensorGroups`: List of sensor groups associated with the checkpoint, including the position of each group.
   -  Attribute type: **Property**. 
   -  Required



# SensorGroup


-  `id`: Unique identifier for the sensor group.
   -  Attribute type: **Property**. 
   -  Required
-  `name`: Name of the sensor group.
   -  Attribute type: **Property**. 
   -  Required
-  `description`: Detailed description of the sensor group.
   -  Attribute type: **Property**. 
   -  Optional
-  `position`: Position of the sensor group in the checkpoint (e.g., before, middle, or after).. One of : `before`, `middle`, `after`.
   -  Attribute type: **Property**. 
   -  Required
-  `sensors`: Array of sensors within the group.
   -  Attribute type: **Property**. 
   -  Required



# Sensor


-  `id`: Unique identifier for the sensor.
   -  Attribute type: **Property**. 
   -  Required
-  `name`: Name of the sensor.
   -  Attribute type: **Property**. 
   -  Required
-  `description`: Detailed description of the sensor.
   -  Attribute type: **Property**. 
   -  Required
-  `type`: Type of the sensor (e.g., "position", "weight", etc.).
   -  Attribute type: **Property**. 
   -  Required
-  `state`: State of the sensor (active or inactive).. One of : `active`, `inactive`.
   -  Attribute type: **Property**. 
   -  Required



## Examples

### A list of factories.



### A list of sectors.



### A list of containers.



### A list of checkpoints.



### A list of sensor groups.



### A list of sensors.


