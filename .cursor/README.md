# Cursor Configuration for NopCommerce .NET 8.0 Migration

This folder contains Cursor AI rules and MCP server configuration for this project.

## Files

### Rules (`.mdc` files in `rules/`)

- **ui-ux-migration-protocol.mdc**: Guidelines for migrating UI/UX from .NET 4.5.1 to 8.0
  - Preserving legacy HTML structure
  - Data loading patterns
  - Image handling
  - View migration workflow

- **workspace-operating-context.mdc**: Core migration principles
  - Minimal changes (migration, not rewrite)
  - Approval workflow
  - Incremental progress
  - Migration diagnostic protocol

### MCP Configuration

- **mcp.json**: Model Context Protocol server configuration (reference)
  - SharpLens: C# code analysis
  - Thinking: Complex decision making
  - Filesystem: File operations

## Usage

### For Team Members

These rules are automatically loaded by Cursor when working in this workspace.

### MCP Setup (Optional)

The `mcp.json` file is a **reference template**. To use MCP servers:

1. Install required tools:
   - SharpLens: Build the SharpToolsMCP project (if available)
   - Thinking: `npx @modelcontextprotocol/server-sequential-thinking`
   - Filesystem: `npx @modelcontextprotocol/server-filesystem`

2. Update paths in your Cursor settings (adjust to your environment):
   ```json
   {
     "mcpServers": {
       "SharpLens": {
         "command": "<your-path>/SharpTools.StdioServer.exe",
         "args": []
       },
       "Thinking": {
         "command": "npx",
         "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"]
       },
       "Filesystem": {
         "command": "npx",
         "args": [
           "-y",
           "@modelcontextprotocol/server-filesystem",
           "<your-workspace-path>"
         ]
       }
     }
   }
   ```

3. Restart Cursor

## Migration Principles

**Key principle from workspace-operating-context.mdc:**

> This is a MIGRATION project, NOT a rewrite project.
> - Keep legacy code, fix ONLY what breaks in .NET 8.0
> - Make minimal changes to achieve compatibility
> - Preserve existing UI/UX structure

**Key principle from ui-ux-migration-protocol.mdc:**

> Adapt legacy code, don't rewrite — minimal changes for maximum compatibility
> - Preserve exact HTML structure
> - Load all data dynamically from database
> - Match legacy visual appearance

---

**These rules ensure consistent migration approach across the team.**
