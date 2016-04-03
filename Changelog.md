# Next release #

> _[See Todo for changes still remaining before release](Todo.md)_


  * FIX		- TthThreaded. It now works as it should again after the Mobile modifications.
  * FIX		- We can now handle hub sending empty commands. [Xmpl - Issue 12](http://code.google.com/p/xmpl/issues/detail?id=12). Thanks Spookie for reporting and investigating. Much easier when someone can point where the error is :)
  * FIX		- Crashed when adding multiple folders to share at once. Thanks hackward for reporting [Issue 19](http://code.google.com/p/flowlib/issues/detail?id=19)
  * FIX		- Crashed when multiple sources to one content was active. Thanks hackward for reporting. [Issue 21](http://code.google.com/p/flowlib/issues/detail?id=21)
  * ADD		- SegmentsDownloaded and SegmentsInProgress properties for DownloadItem.
  * ADD		- function FlowLib.Utils.Convert.General.StringToBitArray (So you can convert string to BitArray).



# 20090103 #

  * **Changes from TODO:**
  * UPnP Support (Rewritten and implemented small parts of my UPnPSharp library to fit FlowLib)
  * Statistics support (Show how much of X traffic that has been transferred and what kind of traffic. You will of course be able to declare your own matching objects.)
  * Connection detection (Code comes from Xmpl). It will make it possible to determine what type of connection user can get without setting some firewall rules up (None, Passive, Active through UPnP, Active).
  * Added UpdateFlowLibMobileProject.exe that will generate a Compact .Net Framework project from the FlowLib.csproj. This is to make it even more easy to use FlowLib on you Mobile platform.




  * **Other changes:**
  * ADD		- Dispose function for TcpConnection and Hub. Disconnects and cleans resources. (You should call this if you dont want hub to reconnect or if you dont want to use the hub object anymore)
    * IsDisposed shows if object has been disposed or not.
    * IsDisposing shows if object is about to be disposed (can be read on ConnectionStatusChange event).
  * FIX		- bug where TcpConnectionListener would throw an objectdisposed exception when calling Start.
  * FIX		- bug where PM would be identified as MainChat message (ADC only).
  * FIX		- bug where Userlist wouldnt be cleared when disconnected from hub.
  * FIX		- bug where hub tried to send/update UserInfo(INF/MyINFO) when connection to hub was not usable.
  * CHANGE	- Nmdc implementation so it is more like Adc implementation. When you send a PM you will receive the same message from hub. **(OBS! This may break your implementation)**
  * ADD/FIX	- (Adc Only) Compare of last INF sent and the one we want to send now and creates a new INF that just have the parts that has been changed. This saves bandwidth and fix so you are not disconnected from some adc hubs (some hubs dont allow sending CID more then once) (Thanks to FleetCommand who pointed me in the right direction).
    * Example:
      * LAST INF:BINF GQEH IDZE4ZCKFL5KEX7H7AXQW2Q7BCVLRG4AODPWJHP5Q PDAOY7W7NO2CEFQVEPQDIAP7UNNMI6L6VQSZPJY3I DE HN0 HO0 HR0 NIXmpl-633523558744590000 SL2 SF0 SS0 VEXmple\sV:20080720
      * Current INF:BINF GQEH IDZE4ZCKFL5KEX7H7AXQW2Q7BCVLRG4AODPWJHP5Q PDAOY7W7NO2CEFQVEPQDIAP7UNNMI6L6VQSZPJY3I DE HN1 HO0 HR0 NIXmpl-633523558744590000 SL2 SF0 SS0 VEXmple\sV:20080720
      * Will give this new INF: BINF GQEH HN1
  * FIX		- Encoding problem when protocol was set to Auto. Thanks to arctic.bot for reporting. [Issue 11](http://code.google.com/p/flowlib/issues/detail?id=11)
  * ADD		- Token in SearchResultInfo (For Adc)
  * CHANGE/FIX		- We are now using codepage 1252 for encoding Nmdc messages (We used system default before. **This may break your implementation**). This means you need to have _codepage 1252_ installed to have Nmdc connection working (make sure you have _libmono-i18n2.0-cil_ installed if you are running Ubuntu+Mono)
  * ADD		- Converting hub settings to/from Xmpl client (http://code.google.com/p/xmpl/)
  * FIX		- SegmentStarted event was not trigged for filelists and files with unknown size. Thanks hackward for reporting. [Issue 14](http://code.google.com/p/flowlib/issues/detail?id=14).
  * CHANGE		- Encoding for protocols can now be set. Thanks hackward for reporting [Issue 15](http://code.google.com/p/flowlib/issues/detail?id=15).
  * FIX		- Bug that made hub count wrong if you was just connected to one ADC hub. Thanks Spookie for reporting.
  * FIX		- Bug that made active searches to send wrong ip to hub.
  * FIX		- ADC search results now includes TTH if available.
  * CHANGE		- All unknown params in search results are now stored so developer can add support for it even if FlowLib doesn't support it yet. (ADC only)
  * FIX		- Bug where search and search results wasn't converted to right format. (ADC Only)
  * CHANGE		- FlowSortedList
    1. Now returns position when a item is added/removed from list.
    1. Added Events for adding, removing and sorting items.
    1. Changed Access level for variables and methods from private to protected.
    1. Changed public Find method so it now uses the Comparer object to determine if it is the same object or not.
  * FIX		- Bug where we used direct instead of echo messages for Private messages in ADC.
  * FIX		- Bug that made tag not being updated with registered and above hub count (ADC Only). Thanks Spookie for reporting.
  * FIX		- Fixed so RCM was sent correctly in ADC. Thanks fadil.khodabuccus for reporting [Issue 16](http://code.google.com/p/flowlib/issues/detail?id=16)
  * FIX		- Bug that made it impossible to parse protocol version on some systems. Thanks fadil.khodabuccus for reporting [Issue 17](http://code.google.com/p/flowlib/issues/detail?id=17)
  * ADD		- EndPoint property to UdpConnection and made properties protected instead of private.
  * CHANGE		- IConnection and IProtocols now implements System.IDisposable and a property called IsDisposed (All objects using them already had them implemented).
  * CHANGE		- Info that is saved in UserInfo.STOREID for nmdc hubs. This id is now unique between hubs. It is created in this format: Hub.HubSetting.Address + Hub.HubSetting.Port + UserInfo.DisplayName. (IF you have saved any stuff with UserInfo.STOREID as key **this may break your implementation. NMDC ONLY**)
  * ADD		- Code Example - What Connection is Possible.
  * ADD		- Code Example - Collect Transfered Information.
  * ADD		- Support for double in General.FormatBytes
  * FIX		- Bug where FlowLib would try to start a secure transfer even if we don't support secure transfers (Only looked at other user before).
  * FIX		- Bugs where User.Id where used instead of User.StoreId in some code examples.
  * FIX		- **FlowLib is now running on Compact Framework 3.5.** Use FlowLibMobile.csproj and FlowLibMobile.sln for developing to your Mobile platform.





# 20080628 #

  * **Changes from TODO:**
    1. Converting hub settings
      * **Special thanks to:**
        * Lightgirl\_xp for supplying xml files.
        * Pothead for reporting bug.
      * DC++
      * DCDM
      * LDC
      * AirDC
      * BCDC
      * CrZDC
      * CZDC
      * fulDC
      * IceDC
      * RSX
      * StrongDC
      * StrongDC Lite
      * Zion Blue
      * zK

  * Support for encrypted connections **(Not for .Net Compact)**
    * Supports ADC0 in ADC hubs (This allows encrypted transfers between DC++ and FlowLib)
    * Supports a modified version of $ConnectToMe and $MyINFO. (Special thanks to PPK who came up with the idea from the begining)
      * `$ConnectToMe <RemoteNick> <SenderIp>:<SenderTLSPort>[S]|` S indicate secure connection and should only be sent from user a if user b also supports TLS, this is indicated in $MyINFO.
      * `$MyINFO $ALL <nick> <description>$ $<connection><flag>$<e-mail>$<sharesize>$|` flag value of: `0x10 = TLS (0x10 hex == 16 dec)` indicates support for tls

  * **Other changes:**
  * ADD		- Extendability for HubSetting
  * REMOVE	- IGuiHub and IGuiMain (Old stuff from FMDC)
  * CHANGE	- IProtocol and TcpConnectionListener now implements IUpdate
  * REMOVE	- IssueReportingGoogleCode from project file (files was never included in svn)
  * CHANGE	- DownloadCompleted, SegmentCompleted, SegmentStarted and SegmentCanceled now have Source as data. This is so you can know who performed the event. Note that Source  for DownloadCompleted is for user who toke the last segment. This doesn't mean that you downloaded all content from this user (if you haven't disabled segment downloading)
  * ADD		- Source to ITransfer and TransferRequest. This is so you can return downloaditems easy.
  * ADD		- StoreID to UserInfo and User. This is so we can save Sources and continue downloading where we started.
  * FIX		- User Tag parsing. Hub count and slot was always -1. (Thanks GhOstFaCE for reporting)
  * FIX		- Format Exception involved with user tag parsing.  (Thanks GhOstFaCE for reporting)
  * FIX		- Own hub count (always showed 0 before) (Thanks GhOstFaCE for reporting)
  * FIX		- Object disposed exception in TcpConnection
  * ADD		- Now updates MyINFO/INF by auto when hubcount or share has changed (Min MyINFO interval is 15 min)
  * FIX		- Out of range exception in Cancel (DownloadItem)
  * FIX		- ConnectToMe can now handle external ip.
  * FIX		- Lock and MyNick was sent multiple times sometimes (Nmdc Transfers).
  * CHANGE	- "V:" is now removed from PK in LOCK command (Nmdc Transfers).
  * FIX		- Fixed uploading (Didn't work when a client wanted to resume download)
  * ADD		- TransferManager now have a property called Requests.
  * FIX		- Shares converted from filelist do now have correct TotalSize, TotalCount,  HashedCount and HashedSize values.
  * FIX		- Bug where sources was not removed when downloaditem was.
  * FIX		- Some [mono](http://www.mono-project.com) fixes
  * FIX		- Crash when receiving SCH and no Share was associated with hub (Adc protocol).
  * ADD		- A copy constructor for UserInfo and TagInfo. This way we can create a copy of the object.
  * FIX		- ADC client to client transfers.
  * FIX		- FlowLib is no longer marked as active if it is passive (ADC only).
  * FIX		- Support tag for client to client transfers (BZIP is added correctly, ADC only)
  * FIX		- [Issue 9](http://code.google.com/p/flowlib/issues/detail?id=9).
  * FIX		- Protocols wasnt disposed correctly. Implemented IDisposable to all Protocols.
  * CHANGE	- HubSettings.Protocol will be updated on Protocol change.
  * FIX		- All Examples are up to date
  * CHANGE	- Not only CID, SID and IP are updated (All extendable info will be updated for user)




# 20080119 #

  * **Changes from TODO:**
    1. Search for content
    1. Receive search result
    1. Adc Support
    1. Add Valid flag to transfer messages and search messages.

  * **Other changes:**
  * ADD		- Active and passive searching (Send/Receive).
    1. FlowLib will not respond with a directory if user is not just searching for directories.
  * FIX		- User Tag parsing. Hub count was always -1. (Thanks GhOstFaCE for reporting)
  * FIX		- Active Transfers, NullReference Exception (Thanks Sneaky for reporting)
  * CHANGE	- You now need to set transfer in Listening state on Actions.TransferStarted and set Event as Handled.
  * FIX		- When Transfer was Disconnected before finished downloading, ArgumentOutOfRange Exception
  * ADD		- Added a RemoveTransfer in TransferManager
  * FIX		- When DownloadItem is changed in Transfer. DownloadItemChanged event is triggered.
  * FIX		- ContentInfo wasnt saved correctly when saving Share
  * FIX		- We will now handle all messages not starting with $ as Mainchat Msg, NMDC. (Thanks Carraya for reporting)
  * CHANGE	- Removed Actions.UsersOnline. We now use Actions.UserOnline instead.  Releated to [Issue 2](http://code.google.com/p/flowlib/issues/detail?id=2)
  * CHANGE	- Removed Actions.OpUsers. We now use Actions.UserOnline or Actions.UserInfoChange. Releated to [Issue 2](http://code.google.com/p/flowlib/issues/detail?id=2)
  * FIX		- Parsing of NickList and OpList made list containing empty usernames.
  * CHANGE	- Actions.UserInfoChange now contains the updated UserInfo. Not only the changes as before.
  * CHANGE	- Actions.UserOffline now contains UserInfo related to User. This is so developers can disconnect transfers and so on.
  * REMOVE	- Removed IP, CID and SID properties from UserInfo. This is because they are not used in all protocols.
  * CHANGE	- Added extendability in UserInfo. It has Reserved names for IP, CID, SID and more. You can use it through Get, Set ,Add, Remove functions.
  * ADD		- Account property in UserInfo.
  * ADD		- Unknown fields in INF command in ADC protocol will be added to user.
  * ADD		- Added Actions.Banned and Actions.Redirect
  * UPDATE	- Updated year in license header for all files to 2008.
  * FIX		- XmlBz2 filelists not having files in it in latest dc++ (TTH tag was wrong)
  * FIX		- Share will now add ending \ to virtualdir if not present.
  * FIX		- Fixed a crash in XmlBz2 filelist creating if not all files was hashed
  * CHANGE	- Hub.StartTransfer has been removed. You should now use UpdateBase event and Actions.StartTransfer.
  * CHANGE	- ShareManager now has the same save/load behavior as Share.
  * ADD		- Adc 1.0 support has been implemented.
  * FIX		- TthThreaded, out of range exception. (Thanks Teobald for reporting. Thanks Carraya for submiting patch)
  * FIX		- ObjectDisposedException in TcpConnectionListener. (Thanks Carraya for reporting)
  * ADD		- Adc 1.0 standard filelist in AddCommonFilelistsToShare
  * ADD		- Adc 1.0 extension BZIP
  * CHANGE	- Update event has been moved from Hub object to IProtocol.
  * ADD		- Share now save/load port and other public properties.
  * CHANGE	- e.Data is now the old protocol(this can be null) on ProtocolChange. This is so you can unlisten on old protocols.
  * FIX		- HashContentInfo event now has ContentInfo as data.
  * ADD		- Download/Upload in ADC (Both 1.0 and 0.10)

# 20071222 #

  * NOTE - First release. Because all started from a project called FMDC we dont have any changelog for this version.