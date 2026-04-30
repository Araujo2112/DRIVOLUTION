package pt.joaoalves03.texpact_wear.dto

import com.fasterxml.jackson.annotation.JsonInclude

@JsonInclude(JsonInclude.Include.NON_NULL)
data class SensorDataDTO(
    val heartRate: Float? = null,
    val steps: Int? = null
)

sealed class SensorData {
    data class HeartRate(val value: Float) : SensorData()
    data class Steps(val value: Int) : SensorData()
}