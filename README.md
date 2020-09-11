# VisualStudioClean

A small software to clean up build objects of Visual Studio and retains only source code before packaging the source code and ship it to somewhere.

It deletes dependencies packages, bin, obj, debug and release folder including all the files inside it, use it with caution.

First place the executable and configuration in root folder of your source code you want to ship, click Scan VS and it will determine which folder to delete and you can exclude folder you want to retain.

The software Start method does not uses threading so when deleting, the program will hang for a while, I am too lazy to enhance it. Someone can branch it out and place threading inside.

Thank you
