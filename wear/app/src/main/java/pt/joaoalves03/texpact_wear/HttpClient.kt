package pt.joaoalves03.texpact_wear

import okhttp3.OkHttpClient
import java.util.concurrent.TimeUnit

val HttpClient: OkHttpClient = OkHttpClient()
    .newBuilder()
    .connectTimeout(30, TimeUnit.SECONDS)
    .readTimeout(30, TimeUnit.SECONDS)
    .build()