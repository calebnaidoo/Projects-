package com.betwise.app.ui.theme

import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

// Casino Dark Theme
val GoldPrimary = Color(0xFFFFD700)
val GoldVariant = Color(0xFFFFB300)
val DarkBg = Color(0xFF121212)
val SurfaceDark = Color(0xFF1E1E1E)
val CardDark = Color(0xFF252525)
val GreenAccent = Color(0xFF4CAF50)
val RedAccent = Color(0xFFE53935)
val TextWhite = Color(0xFFFFFFFF)
val TextGray = Color(0xFFB0B0B0)

private val DarkColorScheme = darkColorScheme(
    primary = GoldPrimary,
    onPrimary = Color.Black,
    primaryContainer = Color(0xFF4A3800),
    onPrimaryContainer = GoldPrimary,
    secondary = GoldVariant,
    onSecondary = Color.Black,
    background = DarkBg,
    onBackground = TextWhite,
    surface = SurfaceDark,
    onSurface = TextWhite,
    surfaceVariant = CardDark,
    onSurfaceVariant = TextGray,
    error = RedAccent,
    onError = Color.White
)

@Composable
fun BetWiseTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = DarkColorScheme,
        typography = Typography(),
        content = content
    )
}
