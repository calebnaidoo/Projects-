package com.betwise.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.Person
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.AuthViewModel

@Composable
fun LoginScreen(
    viewModel: AuthViewModel,
    onLoginSuccess: () -> Unit,
    onNavigateToRegister: () -> Unit
) {
    var username by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    val error by viewModel.authError.collectAsState()

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Brush.verticalGradient(listOf(Color(0xFF0A0A0A), DarkBg)))
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 28.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Spacer(Modifier.height(64.dp))

            Text("🎰", fontSize = 64.sp)
            Spacer(Modifier.height(12.dp))
            Text(
                "BetWise",
                fontSize = 36.sp,
                fontWeight = FontWeight.ExtraBold,
                color = GoldPrimary,
                letterSpacing = 2.sp
            )
            Text(
                "Welcome Back",
                fontSize = 15.sp,
                color = TextGray,
                modifier = Modifier.padding(top = 4.dp)
            )

            Spacer(Modifier.height(48.dp))

            BetWiseCard {
                SectionTitle("SIGN IN")
                Spacer(Modifier.height(20.dp))

                BetWiseTextField(
                    value = username,
                    onValueChange = {
                        username = it
                        viewModel.clearError()
                    },
                    label = "Username",
                    leadingIcon = {
                        Icon(Icons.Default.Person, contentDescription = null, tint = GoldPrimary)
                    }
                )

                Spacer(Modifier.height(14.dp))

                BetWiseTextField(
                    value = password,
                    onValueChange = {
                        password = it
                        viewModel.clearError()
                    },
                    label = "Password",
                    isPassword = true,
                    leadingIcon = {
                        Icon(Icons.Default.Lock, contentDescription = null, tint = GoldPrimary)
                    }
                )

                if (error != null) {
                    Spacer(Modifier.height(10.dp))
                    Text(
                        text = "⚠️ ${error}",
                        color = RedAccent,
                        fontSize = 13.sp,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.fillMaxWidth()
                    )
                }

                Spacer(Modifier.height(24.dp))

                GoldButton(
                    text = "LOGIN",
                    onClick = {
                        viewModel.login(username.trim(), password, onLoginSuccess)
                    },
                    enabled = username.isNotBlank() && password.isNotBlank()
                )
            }

            Spacer(Modifier.height(28.dp))

            Row(
                horizontalArrangement = Arrangement.Center,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text("Don't have an account? ", color = TextGray, fontSize = 14.sp)
                Text(
                    "Register",
                    color = GoldPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = 14.sp,
                    modifier = Modifier.clickable { onNavigateToRegister() }
                )
            }

            Spacer(Modifier.height(32.dp))
        }
    }
}
