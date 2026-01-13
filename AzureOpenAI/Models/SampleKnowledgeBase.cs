namespace AzureOpenAI.Models;

public static class SampleKnowledgeBase
{
    public static List<KnowledgeDocument> GetSampleDocuments()
    {
        return
        [
            // Password Reset Procedures
            new KnowledgeDocument
            {
                Id = "kb-001",
                Title = "Password Reset Policy and Procedure",
                Content = """
                PASSWORD RESET POLICY

                Overview:
                This document outlines the standard procedure for password resets in our organization.

                Self-Service Password Reset:
                1. Users can reset their own passwords through the self-service portal at https://portal.company.com
                2. Requires verification via registered email or phone number
                3. Must meet complexity requirements: minimum 12 characters, uppercase, lowercase, number, and special character

                IT Support Password Reset:
                1. User must verify identity by providing employee ID and answer to security question
                2. Temporary password will be sent to registered email
                3. User must change password on first login

                Security Considerations:
                - Passwords expire every 90 days
                - Cannot reuse last 5 passwords
                - Account locks after 5 failed login attempts
                - Locked accounts automatically unlock after 30 minutes or can be manually unlocked by IT

                Escalation:
                If user cannot access email or phone for verification, escalate to Tier 2 support for manual identity verification.
                """,
                DocumentType = DocumentType.Procedure,
                Category = "Security",
                Tags = ["password", "security", "authentication", "account"],
                LastUpdated = DateTime.UtcNow,
                Source = "IT Security Team",
                Author = "Security Policy Team"
            },

            // VPN Connectivity Issues
            new KnowledgeDocument
            {
                Id = "kb-002",
                Title = "VPN Connection Troubleshooting Guide",
                Content = """
                VPN CONNECTION TROUBLESHOOTING

                Common VPN Issues and Solutions:

                Issue 1: Cannot Connect to VPN
                Symptoms: VPN client shows "Connection Failed" or "Unable to establish connection"
                Solutions:
                1. Verify internet connection is working
                2. Check if VPN client is up to date (current version: 5.2.1)
                3. Ensure firewall allows VPN traffic on ports 443 and 1194
                4. Try connecting to alternate VPN server: vpn2.company.com
                5. Clear VPN cache: Delete contents of C:\ProgramData\VPNClient\cache

                Issue 2: VPN Connects but No Network Access
                Symptoms: VPN shows connected but cannot access internal resources
                Solutions:
                1. Check DNS settings - should use 10.0.1.10 and 10.0.1.11
                2. Verify split tunneling is disabled for full access
                3. Run network diagnostics: Open Command Prompt and run "ipconfig /all" and "nslookup intranet.company.com"
                4. Restart VPN adapter from Network Connections

                Issue 3: Slow VPN Performance
                Symptoms: Very slow connection speeds while on VPN
                Solutions:
                1. Connect to geographically closer VPN server
                2. Check local bandwidth usage
                3. Disable IPv6 in VPN adapter settings
                4. Update network drivers

                Known Issues:
                - VPN client v5.1.x has compatibility issues with Windows 11 - upgrade to v5.2.1
                - MacOS Ventura users must grant Full Disk Access to VPN client

                If none of these solutions work, escalate to Network Team.
                """,
                DocumentType = DocumentType.Troubleshooting,
                Category = "Network",
                Tags = ["vpn", "network", "connectivity", "remote access"],
                LastUpdated = DateTime.UtcNow,
                Source = "Network Operations Team",
                Author = "Network Team"
            },

            // Email Issues
            new KnowledgeDocument
            {
                Id = "kb-003",
                Title = "Email Troubleshooting - Common Issues",
                Content = """
                EMAIL TROUBLESHOOTING GUIDE

                Problem: Cannot Send Emails
                Possible Causes and Solutions:
                1. Mailbox quota exceeded
                   - Check mailbox size in Outlook: File > Info > Cleanup Tools > Mailbox Cleanup
                   - Mailbox limit is 50GB
                   - Solution: Archive old emails or delete unnecessary items

                2. Large attachment size
                   - Attachment size limit is 25MB per email
                   - Solution: Use file sharing service for larger files (SharePoint or OneDrive)

                3. Recipient address blocked
                   - Check if sending to external domain that's blacklisted
                   - Solution: Request whitelist from Security Team

                Problem: Cannot Receive Emails
                Possible Causes and Solutions:
                1. Mailbox full - see mailbox quota solution above
                2. Forwarding rule issue
                   - Check inbox rules: File > Manage Rules & Alerts
                   - Disable suspicious or unknown rules
                3. Emails going to Junk folder
                   - Check Junk Email folder
                   - Add sender to Safe Senders list

                Problem: Email Sync Issues (Mobile)
                Solutions:
                1. Remove and re-add account on mobile device
                2. Ensure using modern authentication (OAuth)
                3. Check if conditional access policy is blocking device
                4. Verify ActiveSync is enabled for user account

                Problem: Missing Emails After Search
                Solutions:
                1. Check Search folder is indexing correctly
                2. Rebuild search index: File > Options > Search > Indexing Options
                3. Check if emails were archived or moved to folders

                Outlook Configuration Settings:
                - Incoming Server: outlook.office365.com (Port 993, SSL)
                - Outgoing Server: smtp.office365.com (Port 587, TLS)
                - Authentication: Modern Authentication (OAuth 2.0)
                """,
                DocumentType = DocumentType.Troubleshooting,
                Category = "Email",
                Tags = ["email", "outlook", "office365", "messaging"],
                LastUpdated = DateTime.UtcNow,
                Source = "Email Support Team",
                Author = "Email Team"
            },

            // Software Installation Policy
            new KnowledgeDocument
            {
                Id = "kb-004",
                Title = "Software Installation and Request Policy",
                Content = """
                SOFTWARE INSTALLATION POLICY

                Purpose:
                To ensure software security, licensing compliance, and system stability.

                Approved Software:
                Users can self-install approved software from the Company Software Portal without IT approval:
                - Microsoft Office Suite
                - Adobe Acrobat Reader
                - Zoom
                - Slack
                - Chrome, Firefox, Edge browsers
                - 7-Zip
                - Notepad++

                Software Request Process:
                For software not on approved list:
                1. Submit request through IT Service Portal
                2. Provide business justification
                3. Include software name, vendor, and version
                4. Expected approval time: 3-5 business days

                Approval Criteria:
                - Business necessity
                - Security review passed
                - License availability
                - Compatibility with existing systems
                - Budget approval for licensed software

                Prohibited Software:
                - Peer-to-peer file sharing applications
                - Unauthorized remote access tools
                - Cryptocurrency mining software
                - Unlicensed or pirated software
                - Software from untrusted sources

                Administrative Rights:
                - Standard users do not have local admin rights
                - Admin rights requests require manager approval
                - Temporary admin rights can be granted for specific installations
                - Privileged Access Management (PAM) system used for temporary elevation

                Software Updates:
                - Critical security updates: Automatic
                - Feature updates: Monthly maintenance window (3rd Sunday, 2-6 AM)
                - Users should not defer updates beyond 7 days

                Bring Your Own License (BYOL):
                - Personal licenses not permitted on company devices
                - All software must be company-licensed
                - Exception: Open source software after security review
                """,
                DocumentType = DocumentType.Policy,
                Category = "Software",
                Tags = ["software", "policy", "installation", "licensing"],
                LastUpdated = DateTime.UtcNow,
                Source = "IT Policy Team",
                Author = "IT Governance"
            },

            // Printer Setup
            new KnowledgeDocument
            {
                Id = "kb-005",
                Title = "Network Printer Setup and Troubleshooting",
                Content = """
                NETWORK PRINTER SETUP GUIDE

                Adding a Network Printer:

                Windows:
                1. Open Settings > Devices > Printers & Scanners
                2. Click "Add a printer or scanner"
                3. Select network printer from the list
                4. If not listed, click "The printer that I want isn't listed"
                5. Enter printer address: \\printserver.company.com\PrinterName
                6. Follow installation wizard

                Mac:
                1. Open System Preferences > Printers & Scanners
                2. Click "+" to add printer
                3. Select from "Default" tab or enter IP address
                4. Choose appropriate driver

                Common Office Printers:
                - 1st Floor Main: \\printserver\Floor1-Main-HP
                - 2nd Floor Color: \\printserver\Floor2-Color-Canon
                - 3rd Floor Main: \\printserver\Floor3-Main-HP

                Common Issues:

                Problem: Printer Offline
                Solutions:
                1. Check printer power and network cable
                2. Remove and re-add printer
                3. Restart print spooler service
                   - Windows: Run "services.msc", restart "Print Spooler"
                   - Mac: Terminal command: sudo launchctl stop org.cups.cupsd

                Problem: Print Jobs Stuck in Queue
                Solutions:
                1. Cancel all documents in queue
                2. Restart print spooler (see above)
                3. Delete contents of: C:\Windows\System32\spool\PRINTERS

                Problem: Poor Print Quality
                Solutions:
                1. Run printer cleaning cycle from printer's menu
                2. Check toner/ink levels
                3. Update printer drivers
                4. Submit maintenance request if issue persists

                Problem: Access Denied
                Solutions:
                1. Verify user has printer access permissions
                2. Check if authentication is required
                3. Re-enter network credentials

                Printer Support Contacts:
                - Print server issues: printteam@company.com
                - Hardware issues: facilities@company.com
                - Toner/supplies: supplies@company.com
                """,
                DocumentType = DocumentType.UserGuide,
                Category = "Hardware",
                Tags = ["printer", "printing", "hardware", "setup"],
                LastUpdated = DateTime.UtcNow,
                Source = "Desktop Support Team",
                Author = "Support Team"
            },

            // Incident Escalation Policy
            new KnowledgeDocument
            {
                Id = "kb-006",
                Title = "Incident Management and Escalation Policy",
                Content = """
                INCIDENT MANAGEMENT POLICY

                Incident Priority Levels:

                P1 - Critical (Response: Immediate, Resolution: 4 hours)
                - Complete system outage affecting all users
                - Security breach or data leak
                - Critical business application down
                - Examples: Email system down, ERP system unavailable, network outage

                P2 - High (Response: 1 hour, Resolution: 8 hours)
                - Major functionality impaired
                - Affecting multiple users or departments
                - Significant performance degradation
                - Examples: VPN issues affecting remote workers, shared drive inaccessible

                P3 - Medium (Response: 4 hours, Resolution: 24 hours)
                - Minor functionality impaired
                - Affecting individual user or small group
                - Workaround available
                - Examples: Single user email issue, printer offline, software installation request

                P4 - Low (Response: 8 hours, Resolution: 5 business days)
                - Minimal impact
                - Informational or enhancement request
                - Examples: Password reset, general questions, feature requests

                Escalation Path:

                Level 1 - Service Desk (Initial Contact)
                - Password resets
                - Basic troubleshooting
                - Software installation from approved list
                - Ticket creation and tracking

                Level 2 - Technical Support
                - Complex troubleshooting
                - System configuration
                - Application support
                - Network connectivity issues

                Level 3 - Specialist Teams
                - Network Team: Infrastructure, VPN, firewall
                - Security Team: Security incidents, access control
                - Database Team: Database issues
                - Application Team: Custom application support

                Level 4 - Vendor Support
                - Issues requiring vendor expertise
                - Hardware RMA
                - Software bugs requiring patch

                When to Escalate:
                - Issue cannot be resolved within SLA timeframe
                - Requires specialized knowledge or access
                - User requests escalation (manager approval required)
                - Multiple related incidents indicate systemic issue

                Escalation Procedure:
                1. Document all troubleshooting steps taken
                2. Update ticket with detailed information
                3. Assign ticket to appropriate team/queue
                4. Notify user of escalation and new expected timeframe
                5. Remain available for questions from escalation team

                After Hours Support:
                - P1/P2 incidents: Call 24/7 hotline: 1-800-555-0123
                - P3/P4 incidents: Submit ticket, handled next business day
                """,
                DocumentType = DocumentType.SOP,
                Category = "Incident Management",
                Tags = ["escalation", "sla", "incident", "priority", "policy"],
                LastUpdated = DateTime.UtcNow,
                Source = "IT Service Management",
                Author = "ITSM Team"
            },

            // MFA Setup
            new KnowledgeDocument
            {
                Id = "kb-007",
                Title = "Multi-Factor Authentication (MFA) Setup Guide",
                Content = """
                MULTI-FACTOR AUTHENTICATION SETUP

                What is MFA?
                Multi-Factor Authentication adds an extra layer of security by requiring a second form of verification beyond your password.

                Required For:
                - All remote access (VPN, OWA, Office 365)
                - Administrative accounts
                - Access to sensitive systems

                Setup Instructions:

                Method 1: Microsoft Authenticator App (Recommended)
                1. Download Microsoft Authenticator from app store
                2. Go to https://aka.ms/mfasetup
                3. Sign in with your company credentials
                4. Click "Set up Authenticator app"
                5. Scan QR code with app
                6. Complete verification

                Method 2: SMS Text Message
                1. Go to https://aka.ms/mfasetup
                2. Select "Phone"
                3. Enter mobile phone number
                4. Choose "Send me a code by text message"
                5. Enter verification code received

                Method 3: Phone Call
                1. Go to https://aka.ms/mfasetup
                2. Select "Phone"
                3. Enter phone number
                4. Choose "Call me"
                5. Answer call and press # to verify

                Using MFA:
                - When signing in, enter password as usual
                - Approve notification in Authenticator app, or
                - Enter code from app/SMS, or
                - Answer phone call and press #

                Trusted Devices:
                - Can mark device as trusted for 30 days
                - Only mark personal devices as trusted
                - Company laptops already configured as trusted

                Troubleshooting:

                Problem: Lost/Changed Phone
                Solution: Contact IT Service Desk for MFA reset
                Verification required: Employee ID, manager confirmation

                Problem: Not Receiving Codes
                Solutions:
                - Check phone signal/data connection
                - Verify phone number is correct in profile
                - Use backup authentication method
                - Request IT to resend activation

                Problem: Authenticator App Out of Sync
                Solutions:
                - Ensure phone time is set to automatic
                - Remove and re-add account in app
                - Use backup codes (provided during setup)

                Backup Codes:
                - 10 one-time use codes provided during setup
                - Store securely (password manager or secure location)
                - Each code works only once
                - Request new codes when running low

                Security Best Practices:
                - Never share MFA codes with anyone, including IT staff
                - Don't approve MFA prompts you didn't initiate
                - Report suspicious MFA requests to security@company.com
                - Keep backup authentication method up to date
                """,
                DocumentType = DocumentType.UserGuide,
                Category = "Security",
                Tags = ["mfa", "authentication", "security", "2fa", "setup"],
                LastUpdated = DateTime.UtcNow,
                Source = "Security Team",
                Author = "InfoSec"
            },

            // Laptop Performance Issues
            new KnowledgeDocument
            {
                Id = "kb-008",
                Title = "Laptop Performance Optimization Guide",
                Content = """
                LAPTOP PERFORMANCE TROUBLESHOOTING

                Common Performance Issues and Solutions:

                Issue: Slow Startup
                Causes and Solutions:
                1. Too many startup programs
                   - Windows: Open Task Manager > Startup tab
                   - Disable unnecessary programs
                   - Keep only: Antivirus, VPN client, essential business apps

                2. Pending updates
                   - Check Windows Update
                   - Install all pending updates
                   - Restart computer

                3. Disk space low
                   - Minimum 20% free space required
                   - Run Disk Cleanup: cleanmgr
                   - Clear temp files, browser cache
                   - Move large files to network storage

                Issue: Computer Running Slow
                Diagnostic Steps:
                1. Check CPU/Memory usage in Task Manager
                2. Identify resource-intensive processes
                3. Close unnecessary applications

                Common Culprits:
                - Browser with too many tabs (> 20 tabs)
                - Multiple video conferencing apps running
                - Large Excel files or data processing
                - Antivirus full system scan

                Solutions:
                - Close unused applications
                - Restart computer (clears memory)
                - Add more RAM if usage consistently > 90% (submit hardware request)
                - Upgrade to SSD if still using HDD

                Issue: High Disk Usage
                Solutions:
                1. Disable Windows Search indexing temporarily
                2. Check for malware scan running
                3. Disable Superfetch service (for HDDs)
                4. Run disk check: chkdsk /f /r

                Issue: Overheating/Fan Noise
                Solutions:
                1. Ensure vents are not blocked
                2. Clean air vents (use compressed air)
                3. Use laptop on hard surface, not soft surfaces
                4. Check if running resource-intensive tasks
                5. Submit hardware request if issue persists

                Issue: Battery Draining Quickly
                Solutions:
                1. Check battery health: powercfg /batteryreport
                2. Reduce screen brightness
                3. Close power-hungry applications
                4. Disable Bluetooth/WiFi when not needed
                5. Use Power Saver mode
                6. Replace battery if health < 70% (submit hardware request)

                Preventive Maintenance:
                - Restart computer at least weekly
                - Keep disk space above 20% free
                - Run Windows Update regularly
                - Close applications when not in use
                - Limit browser tabs to < 20

                Performance Optimization:
                1. Disable visual effects: System > Advanced > Performance Settings
                2. Disable unnecessary background apps
                3. Clear browser cache regularly
                4. Uninstall unused programs
                5. Disable OneDrive sync for large folders not needed offline

                When to Request New Hardware:
                - Computer is > 4 years old
                - RAM consistently maxed out
                - Performance issues persist after optimization
                - Hardware failures (battery, hard drive)

                Hardware Request Process:
                1. Submit ticket with performance metrics
                2. Manager approval required
                3. Backup data to OneDrive/network drive
                4. Schedule device swap with IT
                """,
                DocumentType = DocumentType.Troubleshooting,
                Category = "Hardware",
                Tags = ["laptop", "performance", "slow", "hardware", "optimization"],
                LastUpdated = DateTime.UtcNow,
                Source = "Desktop Support Team",
                Author = "Support Team"
            }
        ];
    }
}