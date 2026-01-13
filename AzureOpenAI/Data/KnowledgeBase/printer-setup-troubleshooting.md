---
id: kb-005
title: Network Printer Setup and Troubleshooting
documentType: UserGuide
category: Hardware
tags: [printer, printing, hardware, setup]
author: Support Team
source: Desktop Support Team
---

# Network Printer Setup Guide

## Adding a Network Printer

### Windows

1. Open Settings > Devices > Printers & Scanners
2. Click "Add a printer or scanner"
3. Select network printer from the list
4. If not listed, click "The printer that I want isn't listed"
5. Enter printer address: \\printserver.company.com\PrinterName
6. Follow installation wizard

### Mac

1. Open System Preferences > Printers & Scanners
2. Click "+" to add printer
3. Select from "Default" tab or enter IP address
4. Choose appropriate driver

## Common Office Printers

- 1st Floor Main: \\printserver\Floor1-Main-HP
- 2nd Floor Color: \\printserver\Floor2-Color-Canon
- 3rd Floor Main: \\printserver\Floor3-Main-HP

## Common Issues

### Problem: Printer Offline

**Solutions:**
1. Check printer power and network cable
2. Remove and re-add printer
3. Restart print spooler service
   - Windows: Run "services.msc", restart "Print Spooler"
   - Mac: Terminal command: sudo launchctl stop org.cups.cupsd

### Problem: Print Jobs Stuck in Queue

**Solutions:**
1. Cancel all documents in queue
2. Restart print spooler (see above)
3. Delete contents of: C:\Windows\System32\spool\PRINTERS

### Problem: Poor Print Quality

**Solutions:**
1. Run printer cleaning cycle from printer's menu
2. Check toner/ink levels
3. Update printer drivers
4. Submit maintenance request if issue persists

### Problem: Access Denied

**Solutions:**
1. Verify user has printer access permissions
2. Check if authentication is required
3. Re-enter network credentials

## Printer Support Contacts

- Print server issues: printteam@company.com
- Hardware issues: facilities@company.com
- Toner/supplies: supplies@company.com
