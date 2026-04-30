package pt.joaoalves03.texpact_wear.presentation.views

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import androidx.wear.compose.material.CircularProgressIndicator
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.Text
import kotlinx.coroutines.launch
import pt.joaoalves03.texpact_wear.R
import pt.joaoalves03.texpact_wear.services.IoTAgentService

@Composable
fun StatusView(navController: NavController, viewModel: StatusViewModel) {
  var isLoading by remember { mutableStateOf(false) }

  val scope = rememberCoroutineScope()

  LaunchedEffect(Unit) {
    isLoading = true
    scope.launch {
      try {
        IoTAgentService.init(navController.context)

        if(IoTAgentService.getDeviceInfo() != null) {
          viewModel.startHealthService()
        }
      } catch (e: Exception) {
        // TODO: Do something with the error
        // Maybe try creating custom exceptions
        println(e)
      } finally {
        isLoading = false
      }
    }
  }

  Column(
    modifier = Modifier.fillMaxWidth(),
    horizontalAlignment = Alignment.CenterHorizontally,
    verticalArrangement = Arrangement.spacedBy(8.dp)
  ) {
    Image(
      painter = painterResource(id = R.drawable.logo_texpact_w),
      contentDescription = "Texpact logo",
      modifier = Modifier.width(128.dp)
    )

    when {
      isLoading -> CircularProgressIndicator()
      else -> Column(
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(2.dp)
      ) {
        Text(
          text = "Connected successfully"
        )

        Spacer(modifier = Modifier.height(8.dp))

        Text(
          text = "Device ID",
          fontSize = 12.sp
        )
        Text(
          color = MaterialTheme.colors.primary,
          fontWeight = FontWeight.Bold,
          text = IoTAgentService.id
        )
      }
    }
  }


}