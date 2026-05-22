# Psycho Discord Setup

Codex cannot create the Discord server without an authenticated Discord account or bot-management connector. Use this template to create the official community server manually.

## Server

- Server name: `Psycho`
- Suggested invite slug: `psycho`
- Community mode: enabled after rules and moderation channels are ready.

## Roles

- Owner
- Administrator
- Developer
- Moderator
- Support
- Content Tester
- Donator
- Member
- Muted
- Bot

## Channels

- `#welcome`
- `#rules`
- `#announcements`
- `#updates`
- `#server-status`
- `#general`
- `#trade`
- `#media`
- `#support`
- `#bug-reports`
- `#suggestions`
- `#staff-chat`
- `#mod-log`
- `#bot-log`
- `#webhook-log`

## Project Environment Variables

Set these on the host before enabling Discord integrations:

```powershell
$env:PSYCHO_DISCORD_BOT_TOKEN = "paste-bot-token-here"
$env:PSYCHO_DISCORD_WEBHOOK_ANNOUNCEMENT = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_STAFF = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_INGAME = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_DEBUG = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_YELL = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_PM = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_CHAT = "paste-webhook-url-here"
$env:PSYCHO_DISCORD_WEBHOOK_CLAN = "paste-webhook-url-here"
```

Legacy `KANDARIN_DISCORD_*` variables still work as fallbacks for local compatibility, but new deployments should use `PSYCHO_DISCORD_*`.

## Bot Permissions

Grant only the permissions needed by the current code:

- View Channels
- Send Messages
- Embed Links
- Read Message History
- Manage Messages, for moderation channels only

Keep the bot token out of source control. Rotate old tokens and webhook URLs if they were ever committed or shared.
