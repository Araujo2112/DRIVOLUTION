package pt.joaoalves03.texpact_wear.services

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.fasterxml.jackson.databind.DeserializationFeature
import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.module.kotlin.jacksonObjectMapper
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.withContext
import okhttp3.HttpUrl.Companion.toHttpUrl
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import pt.joaoalves03.texpact_wear.HttpClient
import pt.joaoalves03.texpact_wear.dto.SensorData
import pt.joaoalves03.texpact_wear.dto.SensorDataDTO
import pt.joaoalves03.texpact_wear.dto.SmartwatchDTO
import pt.joaoalves03.texpact_wear.dto.SmartwatchRegistrationWrapperDTO
import java.util.UUID

val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "settings") // Error 1
private val DEVICE_ID = stringPreferencesKey("device_id")

object IoTAgentService {
  private val mapper: ObjectMapper = jacksonObjectMapper().apply {
    configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
  }
  private const val NORTHBOUND_URL: String = "http://192.168.1.97:4041"
  private const val SOUTHBOUND_URL: String = "http://192.168.1.97:7896"

  var id = ""
    private set

  suspend fun init(context: Context) {
    val deviceId: String = context.dataStore.data
      .map { preferences ->
        preferences[DEVICE_ID] ?: ""
      }.first()

    if(deviceId.isNotEmpty()) {
      id = deviceId

      if(getDeviceInfo() != null) {
        return
      }
    }

    id = UUID.randomUUID().toString().substring(0, 8)

    while (getDeviceInfo() == null) {
      id = UUID.randomUUID().toString().substring(0, 8)
    }

    context.dataStore.edit { preferences ->
      preferences[DEVICE_ID] = id
    }

    registerDevice(id)
  }

  suspend fun getDeviceInfo(): SmartwatchDTO? {
    val request = Request.Builder()
      .url("$NORTHBOUND_URL/iot/devices/smartwatch$id")
      .header("fiware-service", "factory")
      .header("fiware-servicepath", "/")
      .get().build()

    return withContext(Dispatchers.IO) {
      HttpClient.newCall(request).execute().use { response ->
        val responseBody = response.body?.string()
        if (!response.isSuccessful || responseBody == null) {
          null
        }

        mapper.readValue(responseBody, SmartwatchDTO::class.java)
      }
    }
  }

  private suspend fun registerDevice(id: String) {
    val body = SmartwatchRegistrationWrapperDTO(
      devices = listOf(
        SmartwatchDTO(
          "xyz123",
          "smartwatch$id",
          "urn:ngsi-ld:Device:smartwatch$id",
          "Device",
          "Europe/Lisbon"
        )
      )
    )

    val request = Request.Builder()
      .url("$NORTHBOUND_URL/iot/devices")
      .header("fiware-service", "factory")
      .header("fiware-servicepath", "/")
      .post(
        mapper.writeValueAsString(body)
          .toRequestBody("application/json".toMediaTypeOrNull())
      ).build()

    return withContext(Dispatchers.IO) {
      HttpClient.newCall(request).execute().use { response ->
        println(response.body!!.string())
      }
    }
  }

  suspend fun sendSensorData(sensorData: SensorData) {
    val dto = when (sensorData) {
      is SensorData.HeartRate -> SensorDataDTO(heartRate = sensorData.value)
      is SensorData.Steps -> SensorDataDTO(steps = sensorData.value)
    }

    val url = SOUTHBOUND_URL.toHttpUrl().newBuilder()
      .addPathSegment("iot")
      .addPathSegment("json")
      // API Key
      .addQueryParameter("k", "xyz123")
      // Device ID
      .addQueryParameter("i", "smartwatch${this.id}")
      .build()

    //println(url)

    val request = Request.Builder()
      .url(url)
      .header("fiware-service", "factory")
      .header("fiware-servicepath", "/")
      .post(
        mapper.writeValueAsString(dto)
          .toRequestBody("application/json".toMediaTypeOrNull())
      ).build()

    return withContext(Dispatchers.IO) {
      HttpClient.newCall(request).execute().use { response ->
        println(response.body!!.string())
      }
    }
  }
}