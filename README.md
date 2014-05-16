GmailImapClient
===============

Automate parsing of links in Gmail messages through IMAP along with flagging for deletion.

Email verification by GSA Search Engine Ranker does not sufficiently clear my 
gmail catchall mail account so I created this quick bot to parse links automatically
and manage messages to compliment the GSA parser, this leads to a higher verification rate.

Makes use of the MailKit library for IMAP connectivity, and only requests confirmation links
matching the patterns specified which work perfectly for my GSA setup.
