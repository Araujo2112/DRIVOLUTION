/* While this template provides a good starting point for using Wear Compose, you can always
 * take a look at https://github.com/android/wear-os-samples/tree/main/ComposeStarter to find the
 * most up to date changes to the libraries and their usages.
 */

package pt.joaoalves03.texpact_wear

import android.app.Application
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.tooling.preview.Preview
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.tooling.preview.devices.WearDevices
import pt.joaoalves03.texpact_wear.presentation.theme.TexpactwearTheme
import pt.joaoalves03.texpact_wear.presentation.views.StatusView
import pt.joaoalves03.texpact_wear.presentation.views.StatusViewModel

class MainActivity : ComponentActivity() {
  override fun onCreate(savedInstanceState: Bundle?) {
    installSplashScreen()

    super.onCreate(savedInstanceState)

    setTheme(android.R.style.Theme_DeviceDefault)

    //val healthService = HealthService(this)
    //healthService.startWatchingHeartRate()

    setContent {
      WearApp()
    }
  }
}

@Composable
fun WearApp() {
  val navController = rememberNavController()

  TexpactwearTheme {
    Box(
      modifier = Modifier
        .fillMaxSize()
        .background(MaterialTheme.colors.background),
      contentAlignment = Alignment.Center
    ) {
      NavHost(
        navController = navController,
        startDestination = "StatusView",
      ) {
        composable(route = "StatusView") {
          val viewModel: StatusViewModel = viewModel(
            factory = ViewModelProvider.AndroidViewModelFactory.getInstance(LocalContext.current.applicationContext as Application)
          )
          StatusView(navController, viewModel)
        }
      }
    }
  }
}

@Preview(device = WearDevices.SMALL_ROUND, showSystemUi = true)
@Composable
fun DefaultPreview() {
  WearApp()
}