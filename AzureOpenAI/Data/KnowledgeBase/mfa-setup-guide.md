---
id: kb-007
title: Multi-Factor Authentication (MFA) Setup Guide
documentType: UserGuide
category: Security
tags: [mfa, authentication, security, 2fa, setup]
author: InfoSec
source: Security Team
---

# Multi-Factor Authentication Setup

## What is MFA?

Multi-Factor Authentication adds an extra layer of security by requiring a second form of verification beyond your password.

## Required For

- All remote access (VPN, OWA, Office 365)
- Administrative accounts
- Access to sensitive systems

## Setup Instructions

### Method 1: Microsoft Authenticator App (Recommended)

1. Download Microsoft Authenticator from app store
2. Go to https://aka.ms/mfasetup
3. Sign in with your company credentials
4. Click "Set up Authenticator app"
5. Scan QR code with app
6. Complete verification

### Method 2: SMS Text Message

1. Go to https://aka.ms/mfasetup
2. Select "Phone"
3. Enter mobile phone number
4. Choose "Send me a code by text message"
5. Enter verification code received

### Method 3: Phone Call

1. Go to https://aka.ms/mfasetup
2. Select "Phone"
3. Enter phone number
4. Choose "Call me"
5. Answer call and press # to verify

## Using MFA

- When signing in, enter password as usual
- Approve notification in Authenticator app, or
- Enter code from app/SMS, or
- Answer phone call and press #

## Trusted Devices

- Can mark device as trusted for 30 days
- Only mark personal devices as trusted
- Company laptops already configured as trusted

## Troubleshooting

### Problem: Lost/Changed Phone

**Solution:** Contact IT Service Desk for MFA reset
Verification required: Employee ID, manager confirmation

### Problem: Not Receiving Codes

**Solutions:**
- Check phone signal/data connection
- Verify phone number is correct in profile
- Use backup authentication method
- Request IT to resend activation

### Problem: Authenticator App Out of Sync

**Solutions:**
- Ensure phone time is set to automatic
- Remove and re-add account in app
- Use backup codes (provided during setup)

## Backup Codes

- 10 one-time use codes provided during setup
- Store securely (password manager or secure location)
- Each code works only once
- Request new codes when running low

## Security Best Practices

- Never share MFA codes with anyone, including IT staff
- Don't approve MFA prompts you didn't initiate
- Report suspicious MFA requests to security@company.com
- Keep backup authentication method up to date
