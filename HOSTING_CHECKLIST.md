# Hosted Test Checklist

## Build and Runtime

- Use JDK 12 until Gradle is upgraded beyond `5.2.1`.
- Run `.\scripts\build-all.ps1` before every test deployment.
- Launch the server with an explicit config path or `PSYCHO_CONFIG`.
- Keep `data/`, `cache/`, and player-save paths outside disposable build folders.
- Capture `run-logs/server.out.log` and `run-logs/server.err.log`.

## Network

- Default game port is `13377`.
- Only expose the game port needed for the test world.
- Keep database, admin panels, and remote shells firewalled.
- Confirm the client host/port points at the test server before distributing a build.

## Data Safety

- Back up player saves before every deployment.
- Back up config and definitions before mass edits.
- Never deploy test staff accounts with production privileges.
- Verify restart behavior after at least one player login/logout cycle.

## Gameplay Smoke Test

- Login reaches the game world without packet errors.
- Player movement and region loading work around home.
- NPC and object spawn lists load.
- Inventory, bank, equipment, shops, drops, death, and logout persistence work.
- Staff commands are permission-gated.
- Trade, bank, shop, drop, death, and disconnect paths are checked for dupe risk.

## Before Public Testing

- Replace placeholder store/vote URLs and messages.
- Disable or configure Psycho Discord bot integration.
- Review hardcoded rights, usernames, passwords, IPs, and database settings.
- Add economy logging for high-value transfers.
- Create a rollback plan for player saves and config.
