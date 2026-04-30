package pt.joaoalves03.texpact_wear.dto

import com.fasterxml.jackson.annotation.JsonProperty

data class SmartwatchAttributeDTO(
  @JsonProperty("object_id")
  val id: String,
  val name: String,
  val type: String
)

data class SmartwatchRegistrationWrapperDTO(
  val devices: List<SmartwatchDTO>
)

data class SmartwatchDTO(
  @JsonProperty("apikey")
  val apiKey: String,

  @JsonProperty("device_id")
  val deviceId: String,

  @JsonProperty("entity_name")
  val entityName: String,

  @JsonProperty("entity_type")
  val entityType: String,

  val timezone: String?,

  val attributes: List<SmartwatchAttributeDTO> = listOf(
    SmartwatchAttributeDTO(
      id = "heartRate",
      name = "heartRate",
      type = "Property"
    ),
    SmartwatchAttributeDTO(
      id = "steps",
      name = "steps",
      type = "Property"
    )
  )
)