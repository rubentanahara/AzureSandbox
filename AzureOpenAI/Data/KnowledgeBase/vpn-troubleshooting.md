---
id: kb-002
title: VPN Connection Troubleshooting Guide
documentType: Troubleshooting
category: Network
tags: [vpn, network, connectivity, remote access]
author: Network Team
source: Network Operations Team
---

# VPN Connection Troubleshooting Guide

## Common VPN Issues and Solutions

### Issue 1: Cannot Connect to VPN

**Symptoms:** VPN client shows "Connection Failed" or "Unable to establish connection"

**Solutions:**
1. Verify internet connection is working
2. Check if VPN client is up to date (current version: 5.2.1)
3. Ensure firewall allows VPN traffic on ports 443 and 1194
4. Try connecting to alternate VPN server: vpn2.company.com
5. Clear VPN cache: Delete contents of C:\ProgramData\VPNClient\cache

### Issue 2: VPN Connects but No Network Access

**Symptoms:** VPN shows connected but cannot access internal resources

**Solutions:**
1. Check DNS settings - should use 10.0.1.10 and 10.0.1.11
2. Verify split tunneling is disabled for full access
3. Run network diagnostics: Open Command Prompt and run "ipconfig /all" and "nslookup intranet.company.com"
4. Restart VPN adapter from Network Connections

### Issue 3: Slow VPN Performance

**Symptoms:** Very slow connection speeds while on VPN

**Solutions:**
1. Connect to geographically closer VPN server
2. Check local bandwidth usage
3. Disable IPv6 in VPN adapter settings
4. Update network drivers

## Known Issues

- VPN client v5.1.x has compatibility issues with Windows 11 - upgrade to v5.2.1
- MacOS Ventura users must grant Full Disk Access to VPN client

If none of these solutions work, escalate to Network Team.
