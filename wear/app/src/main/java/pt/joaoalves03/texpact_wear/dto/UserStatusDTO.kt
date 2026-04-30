package pt.joaoalves03.texpact_wear.dto

import com.fasterxml.jackson.annotation.JsonInclude

@JsonInclude(JsonInclude.Include.NON_NULL)
data class UserStatusDTO(
  val heartRate: Float? = 0f,
  val steps: Int? = 0
)