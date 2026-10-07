package com.betwise.app.ui.screens

import androidx.compose.foundation.background
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
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.Category
import com.betwise.app.data.entities.Session
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel
import java.time.LocalDate
import java.time.LocalTime
import java.time.format.DateTimeFormatter

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddSessionScreen(viewModel: MainViewModel, onNavigateBack: () -> Unit) {
    val categories by viewModel.categories.collectAsState(initial = emptyList())

    var title by remember { mutableStateOf("") }
    var chipAmount by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var selectedCategory by remember { mutableStateOf<Category?>(null) }
    var date by remember { mutableStateOf(LocalDate.now().toString()) }
    var startTime by remember { mutableStateOf(
        LocalTime.now().format(DateTimeFormatter.ofPattern("HH:mm"))
    ) }
    var endTime by remember { mutableStateOf(
        LocalTime.now().plusHours(1).format(DateTimeFormatter.ofPattern("HH:mm"))
    ) }
    var showCategoryMenu by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("New Session", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Back", tint = TextWhite)
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
                SectionTitle("SESSION DETAILS")
                Spacer(Modifier.height(14.dp))

                BetWiseTextField(
                    value = title,
                    onValueChange = { title = it; error = null },
                    label = "Session Title",
                    leadingIcon = { Icon(Icons.Default.Title, null, tint = GoldPrimary) }
                )

                Spacer(Modifier.height(12.dp))

                OutlinedTextField(
                    value = chipAmount,
                    onValueChange = { if (it.all { c -> c.isDigit() }) { chipAmount = it; error = null } },
                    label = { Text("Chip Amount") },
                    leadingIcon = { Text("🪙", fontSize = 18.sp, modifier = Modifier.padding(start = 12.dp)) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.fillMaxWidth(),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = GoldPrimary,
                        unfocusedBorderColor = Color(0xFF444444),
                        focusedLabelColor = GoldPrimary,
                        unfocusedLabelColor = TextGray,
                        cursorColor = GoldPrimary,
                        focusedTextColor = TextWhite,
                        unfocusedTextColor = TextWhite
                    ),
                    shape = RoundedCornerShape(10.dp),
                    singleLine = true
                )

                Spacer(Modifier.height(12.dp))

                // Category Dropdown
                ExposedDropdownMenuBox(
                    expanded = showCategoryMenu,
                    onExpandedChange = { showCategoryMenu = it }
                ) {
                    OutlinedTextField(
                        value = selectedCategory?.let { "${it.icon} ${it.name}" } ?: "Select Category",
                        onValueChange = {},
                        readOnly = true,
                        label = { Text("Category") },
                        trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = showCategoryMenu) },
                        modifier = Modifier
                            .fillMaxWidth()
                            .menuAnchor(),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedBorderColor = GoldPrimary,
                            unfocusedBorderColor = Color(0xFF444444),
                            focusedLabelColor = GoldPrimary,
                            unfocusedLabelColor = TextGray,
                            focusedTextColor = TextWhite,
                            unfocusedTextColor = if (selectedCategory != null) TextWhite else TextGray
                        ),
                        shape = RoundedCornerShape(10.dp)
                    )
                    ExposedDropdownMenu(
                        expanded = showCategoryMenu,
                        onDismissRequest = { showCategoryMenu = false },
                        modifier = Modifier.background(SurfaceDark)
                    ) {
                        if (categories.isEmpty()) {
                            DropdownMenuItem(
                                text = { Text("No categories — add one first", color = TextGray) },
                                onClick = { showCategoryMenu = false }
                            )
                        } else {
                            categories.forEach { cat ->
                                DropdownMenuItem(
                                    text = { Text("${cat.icon} ${cat.name}", color = TextWhite) },
                                    onClick = { selectedCategory = cat; showCategoryMenu = false }
                                )
                            }
                        }
                    }
                }
            }

            BetWiseCard {
                SectionTitle("DATE & TIME")
                Spacer(Modifier.height(14.dp))

                BetWiseTextField(
                    value = date,
                    onValueChange = { date = it },
                    label = "Date (yyyy-MM-dd)",
                    leadingIcon = { Icon(Icons.Default.CalendarToday, null, tint = GoldPrimary) }
                )
                Spacer(Modifier.height(12.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    OutlinedTextField(
                        value = startTime,
                        onValueChange = { startTime = it },
                        label = { Text("Start (HH:mm)") },
                        modifier = Modifier.weight(1f),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedBorderColor = GoldPrimary,
                            unfocusedBorderColor = Color(0xFF444444),
                            focusedLabelColor = GoldPrimary,
                            unfocusedLabelColor = TextGray,
                            cursorColor = GoldPrimary,
                            focusedTextColor = TextWhite,
                            unfocusedTextColor = TextWhite
                        ),
                        shape = RoundedCornerShape(10.dp),
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = endTime,
                        onValueChange = { endTime = it },
                        label = { Text("End (HH:mm)") },
                        modifier = Modifier.weight(1f),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedBorderColor = GoldPrimary,
                            unfocusedBorderColor = Color(0xFF444444),
                            focusedLabelColor = GoldPrimary,
                            unfocusedLabelColor = TextGray,
                            cursorColor = GoldPrimary,
                            focusedTextColor = TextWhite,
                            unfocusedTextColor = TextWhite
                        ),
                        shape = RoundedCornerShape(10.dp),
                        singleLine = true
                    )
                }
            }

            BetWiseCard {
                SectionTitle("NOTES")
                Spacer(Modifier.height(12.dp))
                OutlinedTextField(
                    value = description,
                    onValueChange = { description = it },
                    label = { Text("Description / Notes") },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(110.dp),
                    maxLines = 4,
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = GoldPrimary,
                        unfocusedBorderColor = Color(0xFF444444),
                        focusedLabelColor = GoldPrimary,
                        unfocusedLabelColor = TextGray,
                        cursorColor = GoldPrimary,
                        focusedTextColor = TextWhite,
                        unfocusedTextColor = TextWhite
                    ),
                    shape = RoundedCornerShape(10.dp)
                )
            }

            if (error != null) {
                Text("⚠️ $error", color = RedAccent, fontSize = 13.sp)
            }

            GoldButton(
                text = "SAVE SESSION",
                onClick = {
                    val chips = chipAmount.toIntOrNull()
                    when {
                        title.isBlank() -> error = "Please enter a session title"
                        chips == null || chips <= 0 -> error = "Please enter a valid chip amount"
                        selectedCategory == null -> error = "Please select a category"
                        date.isBlank() -> error = "Please enter a date"
                        else -> {
                            viewModel.addSession(
                                Session(
                                    userId = viewModel.userId,
                                    categoryId = selectedCategory!!.id,
                                    title = title.trim(),
                                    chipAmount = chips,
                                    date = date,
                                    startTime = startTime,
                                    endTime = endTime,
                                    description = description.trim()
                                )
                            )
                            onNavigateBack()
                        }
                    }
                }
            )
            Spacer(Modifier.height(16.dp))
        }
    }
}
