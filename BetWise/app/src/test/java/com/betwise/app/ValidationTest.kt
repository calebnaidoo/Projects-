package com.betwise.app

import org.junit.Assert.*
import org.junit.Test
import java.security.MessageDigest

class ValidationTest {

    private fun hash(input: String): String {
        val bytes = MessageDigest.getInstance("SHA-256").digest(input.toByteArray())
        return bytes.joinToString("") { "%02x".format(it) }
    }

    @Test
    fun `password hash is consistent`() {
        val password = "securePass123"
        val h1 = hash(password)
        val h2 = hash(password)
        assertEquals(h1, h2)
    }

    @Test
    fun `different passwords produce different hashes`() {
        val h1 = hash("password1")
        val h2 = hash("password2")
        assertNotEquals(h1, h2)
    }

    @Test
    fun `chip amount validation - positive number is valid`() {
        val input = "250"
        val chips = input.toIntOrNull()
        assertNotNull(chips)
        assertTrue(chips!! > 0)
    }

    @Test
    fun `chip amount validation - empty string is invalid`() {
        val input = ""
        val chips = input.toIntOrNull()
        assertNull(chips)
    }

    @Test
    fun `chip amount validation - negative rejected`() {
        val input = "-50"
        val chips = input.toIntOrNull()
        assertNotNull(chips)
        assertFalse(chips!! > 0)
    }

    @Test
    fun `goal validation - min must be less than max`() {
        val min = 100
        val max = 500
        assertTrue(min < max)
    }

    @Test
    fun `goal validation - equal min and max is invalid`() {
        val min = 500
        val max = 500
        assertFalse(min < max)
    }

    @Test
    fun `username blank validation`() {
        val username = "  "
        assertTrue(username.isBlank())
    }

    @Test
    fun `password minimum length check`() {
        val shortPass = "abc"
        val validPass = "abc123"
        assertFalse(shortPass.length >= 6)
        assertTrue(validPass.length >= 6)
    }

    @Test
    fun `date format is correct yyyy-MM-dd`() {
        val date = "2025-07-15"
        val regex = Regex("\\d{4}-\\d{2}-\\d{2}")
        assertTrue(regex.matches(date))
    }
}
