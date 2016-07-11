ZenDesk Ticket Incremental Exporter
===================================

A Windows console application for keeping a local copy of your [Zendesk](https://www.zendesk.com/) tickets updated in an application-managed SQLite database, with optional export to csv.

Prerequisites
-------------
- .NET Framework 4.5.1 installed.

Download and install
--------------------

We use GitHub releases to distribute this application. **[View the latest release](https://github.com/ritterim/zendesk-ticket-exporter/releases/latest).**

Extract the contents of the .zip file to a location of your choice, which will include `ZendeskTicketExporter.Console.exe`.

Example usages:
---------------
Create a new local copy:

`ZendeskTicketExporter.Console.exe -s mysitename -u admin@example.com -t myapitoken -n`

Create a new local copy (long form):

`ZendeskTicketExporter.Console.exe --sitename mysitename --username admin@example.com --token myapitoken --new-database`

Update an existing local copy:

`ZendeskTicketExporter.Console.exe -s mysitename -u admin@example.com -t myapitoken`

Update an existing local copy and output csv:

`ZendeskTicketExporter.Console.exe -s mysitename -u admin@example.com -t myapitoken -e output.csv`

Update an existing local copy and output csv with csv overwrite permitted:

`ZendeskTicketExporter.Console.exe -s mysitename -u admin@example.com -t myapitoken -e output.csv -o`

Update an existing local copy and output csv to UNC path:

`ZendeskTicketExporter.Console.exe -s mysitename -u admin@example.com -t myapitoken -e \\machinename\sharename\output.csv`

Rolling CSV exports using Powershell:

```
$timestamp = $(Get-Date).ToUniversalTime().ToString("yyyy-MM-dd-hhmmss")
$csvOutputPath = "\\machinename\sharename\$timestamp-utc.csv"
.\ZendeskTicketExporter.Console.exe -s mysitename -u admin@example.com -t myapitoken -e $csvOutputPath
```

Options:
--------
```
-s, --sitename                     Required. Sitename for accessing Zendesk
                                   API ([your-site-name].zendesk.com).

-u, --username                     Required. Username for accessing Zendesk
                                   API (do not include "/token").

-t, --token                        Required. API token for accessing Zendesk
                                   API. You can enable and view this at
                                   https://[your-site-name].zendesk.com/settin
                                   gs/api

-n, --new-database                 Permit creation of a new database, does
                                   not permit refresh of existing database.
                                   This is to ensure compliance with the
                                   Zendesk API guidelines.

-e, --export-csv-file              Path to CVS export file, if not specified
                                   no CSV export will be performed.

-o, --export-csv-file-overwrite    Permit overwriting of export-csv-file.

-q, --quiet                        Suppress console logging output.
```
