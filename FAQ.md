# Introduction #

**What should you do if you have a question?**
  1. Read all Questions and responses below.
  1. If you want to know how you should do something. Please look [here](CodeExamples.md) first
  1. Add comment below and your question will be answered (Or removed)
# Questions #


**How do i compile FlowLib for .Net Compact framework?**
  * Change references so they point to .Net Compact.
  * Change project type/platform to .Net Compact (if you dont know how todo it manualy you can create a new project and then import all files from FlowLib.)
  * Define constant "COMPACT\_FRAMEWORK". You are doing this on project level in Visual Studio. (This is so functionality that is not supported by compact framework will not be included)