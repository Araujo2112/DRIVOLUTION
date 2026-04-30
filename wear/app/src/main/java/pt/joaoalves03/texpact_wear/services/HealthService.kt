package pt.joaoalves03.texpact_wear.services

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.Service
import android.content.Intent
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager
import android.os.IBinder
import androidx.core.app.NotificationCompat
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch
import pt.joaoalves03.texpact_wear.dto.SensorData

class HealthService: Service(), SensorEventListener {
    private lateinit var sensorManager: SensorManager
    private var heartRateSensor: Sensor? = null
    private var stepsSensor: Sensor? = null
    private val serviceScope = CoroutineScope(Dispatchers.IO + Job())

    companion object {
        private const val NOTIFICATION_ID = 1
        private const val CHANNEL_ID = "HealthServiceChannel"
    }

    override fun onCreate() {
        super.onCreate()
        setupSensor()
        createNotificationChannel()
        startForeground(NOTIFICATION_ID, createNotification())
    }

    private fun createNotificationChannel() {
        val channel = NotificationChannel(
            CHANNEL_ID,
            "TexP@ct Health Monitoring",
            NotificationManager.IMPORTANCE_LOW
        )
        val notificationManager = getSystemService(NotificationManager::class.java)
        notificationManager.createNotificationChannel(channel)
    }

    private fun createNotification(): Notification {
        return NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle("TexP@ct")
            .setContentText("Monitoring your health")
            .setSmallIcon(android.R.drawable.ic_dialog_info)
            .setPriority(NotificationCompat.PRIORITY_LOW)
            .build()
    }

    private fun setupSensor() {
        sensorManager = getSystemService(SENSOR_SERVICE) as SensorManager
        heartRateSensor = sensorManager.getDefaultSensor(Sensor.TYPE_HEART_RATE)
        heartRateSensor?.let {
            sensorManager.registerListener(
                this,
                it,
                SensorManager.SENSOR_DELAY_NORMAL
            )
        }
        stepsSensor = sensorManager.getDefaultSensor(Sensor.TYPE_STEP_COUNTER)
        stepsSensor?.let {
            sensorManager.registerListener(
                this,
                it,
                SensorManager.SENSOR_DELAY_NORMAL
            )
        }
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onSensorChanged(event: SensorEvent?) {
        serviceScope.launch {
            if (event != null) {
                when(event.sensor.type) {
                    Sensor.TYPE_HEART_RATE -> {
                        IoTAgentService.sendSensorData(SensorData.HeartRate(event.values[0]))
                    }
                    Sensor.TYPE_STEP_COUNTER -> {
                        IoTAgentService.sendSensorData(SensorData.Steps(event.values[0].toInt()))
                    }
                }
            }
        }
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {}
}