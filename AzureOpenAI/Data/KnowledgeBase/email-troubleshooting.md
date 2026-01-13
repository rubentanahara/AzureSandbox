---
id: kb-003
title: Email Troubleshooting - Common Issues
documentType: Troubleshooting
category: Email
tags: [email, outlook, office365, messaging]
author: Email Team
source: Email Support Team
---

# Email Troubleshooting Guide

## Problem: Cannot Send Emails

### Possible Causes and Solutions

#### 1. Mailbox quota exceeded
- Check mailbox size in Outlook: File > Info > Cleanup Tools > Mailbox Cleanup
- Mailbox limit is 50GB
- Solution: Archive old emails or delete unnecessary items

#### 2. Large attachment size
- Attachment size limit is 25MB per email
- Solution: Use file sharing service for larger files (SharePoint or OneDrive)

#### 3. Recipient address blocked
- Check if sending to external domain that's blacklisted
- Solution: Request whitelist from Security Team

## Problem: Cannot Receive Emails

### Possible Causes and Solutions

1. Mailbox full - see mailbox quota solution above
2. Forwarding rule issue
   - Check inbox rules: File > Manage Rules & Alerts
   - Disable suspicious or unknown rules
3. Emails going to Junk folder
   - Check Junk Email folder
   - Add sender to Safe Senders list

## Problem: Email Sync Issues (Mobile)

**Solutions:**
1. Remove and re-add account on mobile device
2. Ensure using modern authentication (OAuth)
3. Check if conditional access policy is blocking device
4. Verify ActiveSync is enabled for user account

## Problem: Missing Emails After Search

**Solutions:**
1. Check Search folder is indexing correctly
2. Rebuild search index: File > Options > Search > Indexing Options
3. Check if emails were archived or moved to folders

## Outlook Configuration Settings

- Incoming Server: outlook.office365.com (Port 993, SSL)
- Outgoing Server: smtp.office365.com (Port 587, TLS)
- Authentication: Modern Authentication (OAuth 2.0)
