package com.betwise.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.Category
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CategoriesScreen(viewModel: MainViewModel, onNavigate: (String) -> Unit) {
    val categories by viewModel.categories.collectAsState(initial = emptyList())
    var showAddDialog by remember { mutableStateOf(false) }

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("Categories", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark),
                actions = {
                    IconButton(onClick = { showAddDialog = true }) {
                        Icon(Icons.Default.Add, contentDescription = "Add", tint = GoldPrimary)
                    }
                }
            )
        },
        bottomBar = { BetWiseBottomBar(currentRoute = "categories", onNavigate = onNavigate) }
    ) { padding ->
        if (categories.isEmpty()) {
            Box(
                modifier = Modifier.fillMaxSize().padding(padding),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("🃏", fontSize = 56.sp)
                    Spacer(Modifier.height(12.dp))
                    Text("No categories yet", color = TextGray, fontSize = 16.sp)
                    Spacer(Modifier.height(8.dp))
                    GoldButton(
                        text = "Add Your First Category",
                        onClick = { showAddDialog = true },
                        modifier = Modifier.width(220.dp)
                    )
                }
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
                    .padding(16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                item { SectionTitle("YOUR CATEGORIES") }
                items(categories) { cat ->
                    CategoryItem(
                        category = cat,
                        onDelete = { viewModel.deleteCategory(cat) }
                    )
                }
                item { Spacer(Modifier.height(8.dp)) }
            }
        }
    }

    if (showAddDialog) {
        AddCategoryDialog(
            onAdd = { name, icon ->
                viewModel.addCategory(name, icon)
                showAddDialog = false
            },
            onDismiss = { showAddDialog = false }
        )
    }
}

@Composable
private fun CategoryItem(category: Category, onDelete: () -> Unit) {
    var showConfirm by remember { mutableStateOf(false) }

    Card(
        colors = CardDefaults.cardColors(containerColor = CardDark),
        shape = RoundedCornerShape(14.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        Row(
            modifier = Modifier.padding(16.dp).fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier
                    .size(48.dp)
                    .background(
                        color = GoldPrimary.copy(alpha = 0.15f),
                        shape = RoundedCornerShape(12.dp)
                    ),
                contentAlignment = Alignment.Center
            ) {
                Text(category.icon, fontSize = 24.sp)
            }
            Spacer(Modifier.width(14.dp))
            Text(
                category.name,
                color = TextWhite,
                fontSize = 16.sp,
                fontWeight = FontWeight.SemiBold,
                modifier = Modifier.weight(1f)
            )
            IconButton(onClick = { showConfirm = true }) {
                Icon(Icons.Default.Delete, contentDescription = "Delete", tint = RedAccent.copy(alpha = 0.7f))
            }
        }
    }

    if (showConfirm) {
        AlertDialog(
            onDismissRequest = { showConfirm = false },
            containerColor = SurfaceDark,
            title = { Text("Delete Category?", color = TextWhite) },
            text = { Text("Delete '${category.name}'? This cannot be undone.", color = TextGray) },
            confirmButton = {
                TextButton(onClick = { onDelete(); showConfirm = false }) {
                    Text("Delete", color = RedAccent)
                }
            },
            dismissButton = {
                TextButton(onClick = { showConfirm = false }) {
                    Text("Cancel", color = TextGray)
                }
            }
        )
    }
}

@Composable
private fun AddCategoryDialog(onAdd: (String, String) -> Unit, onDismiss: () -> Unit) {
    var name by remember { mutableStateOf("") }
    var selectedIcon by remember { mutableStateOf("🎰") }
    val icons = listOf("🎰", "♠️", "🎯", "🃏", "🏈", "🎲", "🌟", "💎", "🚀", "🎳")

    AlertDialog(
        onDismissRequest = onDismiss,
        containerColor = SurfaceDark,
        title = { Text("New Category", color = GoldPrimary, fontWeight = FontWeight.Bold) },
        text = {
            Column {
                BetWiseTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = "Category Name"
                )
                Spacer(Modifier.height(16.dp))
                Text("Choose Icon", color = TextGray, fontSize = 13.sp)
                Spacer(Modifier.height(8.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    icons.take(5).forEach { icon ->
                        IconChoice(icon, icon == selectedIcon) { selectedIcon = icon }
                    }
                }
                Spacer(Modifier.height(8.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    icons.drop(5).forEach { icon ->
                        IconChoice(icon, icon == selectedIcon) { selectedIcon = icon }
                    }
                }
            }
        },
        confirmButton = {
            TextButton(
                onClick = { if (name.isNotBlank()) onAdd(name.trim(), selectedIcon) },
                enabled = name.isNotBlank()
            ) {
                Text("Add", color = GoldPrimary, fontWeight = FontWeight.Bold)
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) { Text("Cancel", color = TextGray) }
        }
    )
}

@Composable
private fun IconChoice(icon: String, selected: Boolean, onClick: () -> Unit) {
    Card(
        onClick = onClick,
        colors = CardDefaults.cardColors(
            containerColor = if (selected) GoldPrimary.copy(alpha = 0.2f) else CardDark
        ),
        shape = RoundedCornerShape(8.dp)
    ) {
        Box(
            modifier = Modifier.size(40.dp),
            contentAlignment = Alignment.Center
        ) {
            Text(icon, fontSize = 20.sp)
        }
    }
}
