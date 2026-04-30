package pt.joaoalves03.texpact_wear.presentation.views

import android.app.Application
import android.content.Intent
import androidx.lifecycle.AndroidViewModel
import pt.joaoalves03.texpact_wear.services.HealthService

class StatusViewModel (private val application: Application) : AndroidViewModel(application) {
    fun startHealthService() {
        val context = getApplication<Application>()
        context.startForegroundService(Intent(context, HealthService::class.java))
    }
}