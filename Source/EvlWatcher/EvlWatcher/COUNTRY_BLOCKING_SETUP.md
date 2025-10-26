# Country Blocking Setup Guide

## Overview
EvlWatcher now supports country-based IP blocking using MaxMind's GeoLite2 database. This feature allows you to block IPs from specific countries automatically.

## Setup Instructions

### 1. Download GeoLite2 Database
1. Visit [MaxMind GeoLite2](https://dev.maxmind.com/geoip/geolite2-free-geolocation-data)
2. Create a free MaxMind account if you don't have one
3. Download the GeoLite2-Country.mmdb file
4. Place the file in the same directory as EvlWatcher.exe (typically `C:\Program Files\EvlWatcher\`)

### 2. Enable Country Blocking
1. Open the EvlWatcher Console
2. Go to the "Country Blocking" tab
3. Check "Enable Country Blocking"
4. Add country codes to block (e.g., CN, RU, KP, IR)
5. Click "Apply Rules to Existing Bans" to apply to current bans

### 3. Country Code Format
- Use ISO 3166-1 alpha-2 country codes (2 letters)
- Examples: CN (China), RU (Russia), KP (North Korea), IR (Iran)
- Case insensitive - will be converted to uppercase

## Features

### Automatic Country Detection
- IPs are automatically checked against the GeoLite2 database
- Country information is displayed in the IP lists
- Format: "192.168.1.1 (US)" or "10.0.0.1 (Unknown)"

### Country Blocking Logic
- When country blocking is enabled, IPs from blocked countries are not added to the firewall ban list
- Existing bans from blocked countries can be removed using "Apply Rules to Existing Bans"
- Country blocking works alongside whitelist patterns

### Configuration
- Settings are stored in config.xml
- Country blocking can be enabled/disabled via console
- Blocked countries list is persistent across service restarts

## Troubleshooting

### Database Not Found
If you see "GeoLite2-Country.mmdb database file not found" in the logs:
1. Ensure the database file is in the same directory as EvlWatcher.exe
2. Check file permissions - the service needs read access
3. Verify the file is not corrupted

### Country Lookup Failures
- "Unknown" country means the IP could not be geolocated
- This is normal for private IPs (192.168.x.x, 10.x.x.x, etc.)
- Some IPs may not be in the database

### Performance Considerations
- Country lookups are cached by MaxMind
- Database file is ~50MB
- Lookup performance is very fast (microseconds)

## Database Updates
- **Automatic Updates**: EvlWatcher now automatically downloads and updates the GeoLite2 database
- **Update Schedule**: Database is checked for updates every 24 hours
- **Backup Protection**: Automatic backup before updates
- **Fallback**: If auto-update fails, manual download instructions are provided in logs
- **Manual Updates**: You can still manually download and replace the database file if needed

## Security Notes
- Country blocking is a best-effort feature
- VPNs and proxies can bypass country-based blocking
- Use in combination with other security measures
- Monitor logs for blocked country attempts

## Common Country Codes
- CN: China
- RU: Russia  
- KP: North Korea
- IR: Iran
- VN: Vietnam
- IN: India
- BR: Brazil
- UA: Ukraine
