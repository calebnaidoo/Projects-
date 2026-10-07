package com.betwise.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.User
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(user: User, onLogout: () -> Unit, onNavigateBack: () -> Unit) {
    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("Settings", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.Default.ArrowBack, null, tint = TextWhite)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark)
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp)
        ) {
            BetWiseCard {
                SectionTitle("PROFILE")
                Spacer(Modifier.height(14.dp))
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text("👤", fontSize = 40.sp)
                    Spacer(Modifier.width(16.dp))
                    Column {
                        Text(user.fullName, color = TextWhite, fontWeight = FontWeight.Bold, fontSize = 16.sp)
                        Text("@${user.username}", color = TextGray, fontSize = 13.sp)
                        Text(user.contactDetail, color = TextGray, fontSize = 13.sp)
                    }
                }
            }

            BetWiseCard {
                SectionTitle("APP INFO")
                Spacer(Modifier.height(12.dp))
                InfoRow("App", "BetWise")
                InfoRow("Version", "1.0.0")
                InfoRow("Purpose", "Educational")
                InfoRow("Module", "PROG7313")
            }

            BetWiseCard {
                SectionTitle("⚠️ DISCLAIMER")
                Spacer(Modifier.height(10.dp))
                Text(
                    "BetWise does not involve real gambling or real money. " +
                    "It is a simulation app designed purely for learning Android development.",
                    color = TextGray,
                    fontSize = 13.sp
                )
            }

            Spacer(Modifier.height(8.dp))

            Button(
                onClick = onLogout,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(52.dp),
                colors = ButtonDefaults.buttonColors(containerColor = RedAccent),
                shape = RoundedCornerShape(12.dp)
            ) {
                Icon(Icons.Default.Logout, null, tint = Color.White)
                Spacer(Modifier.width(8.dp))
                Text("LOGOUT", fontWeight = FontWeight.Bold, color = Color.White)
            }
        }
    }
}

@Composable
private fun InfoRow(label: String, value: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(label, color = TextGray, fontSize = 13.sp)
        Text(value, color = TextWhite, fontSize = 13.sp, fontWeight = FontWeight.Medium)
    }
}
