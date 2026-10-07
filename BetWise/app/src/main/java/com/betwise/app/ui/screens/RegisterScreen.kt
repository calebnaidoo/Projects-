package com.betwise.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
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
fun RegisterScreen(
    viewModel: AuthViewModel,
    onRegisterSuccess: () -> Unit,
    onNavigateToLogin: () -> Unit
) {
    var fullName by remember { mutableStateOf("") }
    var username by remember { mutableStateOf("") }
    var contact by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }
    var localError by remember { mutableStateOf<String?>(null) }
    val authError by viewModel.authError.collectAsState()

    val displayError = localError ?: authError

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
            Spacer(Modifier.height(48.dp))

            Text("🎲", fontSize = 56.sp)
            Spacer(Modifier.height(8.dp))
            Text(
                "Create Account",
                fontSize = 28.sp,
                fontWeight = FontWeight.ExtraBold,
                color = GoldPrimary
            )
            Text("Join BetWise today", fontSize = 14.sp, color = TextGray)

            Spacer(Modifier.height(32.dp))

            BetWiseCard {
                SectionTitle("YOUR DETAILS")
                Spacer(Modifier.height(16.dp))

                BetWiseTextField(
                    value = fullName,
                    onValueChange = { fullName = it; localError = null; viewModel.clearError() },
                    label = "Full Name",
                    leadingIcon = { Icon(Icons.Default.Person, null, tint = GoldPrimary) }
                )
                Spacer(Modifier.height(12.dp))

                BetWiseTextField(
                    value = username,
                    onValueChange = { username = it; localError = null; viewModel.clearError() },
                    label = "Username",
                    leadingIcon = { Icon(Icons.Default.AccountCircle, null, tint = GoldPrimary) }
                )
                Spacer(Modifier.height(12.dp))

                BetWiseTextField(
                    value = contact,
                    onValueChange = { contact = it; localError = null; viewModel.clearError() },
                    label = "Email / Phone",
                    leadingIcon = { Icon(Icons.Default.Email, null, tint = GoldPrimary) }
                )
                Spacer(Modifier.height(12.dp))

                BetWiseTextField(
                    value = password,
                    onValueChange = { password = it; localError = null; viewModel.clearError() },
                    label = "Password",
                    isPassword = true,
                    leadingIcon = { Icon(Icons.Default.Lock, null, tint = GoldPrimary) }
                )
                Spacer(Modifier.height(12.dp))

                BetWiseTextField(
                    value = confirmPassword,
                    onValueChange = { confirmPassword = it; localError = null; viewModel.clearError() },
                    label = "Confirm Password",
                    isPassword = true,
                    leadingIcon = { Icon(Icons.Default.Lock, null, tint = GoldPrimary) }
                )

                if (displayError != null) {
                    Spacer(Modifier.height(10.dp))
                    Text(
                        text = "⚠️ $displayError",
                        color = RedAccent,
                        fontSize = 13.sp,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.fillMaxWidth()
                    )
                }

                Spacer(Modifier.height(24.dp))

                GoldButton(
                    text = "CREATE ACCOUNT",
                    onClick = {
                        when {
                            fullName.isBlank() -> localError = "Please enter your full name"
                            username.isBlank() -> localError = "Please choose a username"
                            contact.isBlank() -> localError = "Please enter a contact detail"
                            password.length < 6 -> localError = "Password must be at least 6 characters"
                            password != confirmPassword -> localError = "Passwords do not match"
                            else -> viewModel.register(
                                fullName.trim(), username.trim(), contact.trim(), password, onRegisterSuccess
                            )
                        }
                    }
                )
            }

            Spacer(Modifier.height(24.dp))
            Row(verticalAlignment = Alignment.CenterVertically) {
                Text("Already have an account? ", color = TextGray, fontSize = 14.sp)
                Text(
                    "Sign In",
                    color = GoldPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = 14.sp,
                    modifier = Modifier.clickable { onNavigateToLogin() }
                )
            }
            Spacer(Modifier.height(40.dp))
        }
    }
}
