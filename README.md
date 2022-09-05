# SECRET and SecretEngine

## About SECRET

SECRET is a command line encryption tool for Windows. 
It comes as a standalone binary file which does not need installation. 
Copy the file to some path and run it from there. 

Note that you will need .NET 6 installed for SECRET to work.

SECRET is very easy to use and can handle encryption of very large files. 
It supports all file types. SECRET also allows you to encrypt directories with ease, even recursively. 

## SecretEngine

SecretEngine is reliable, yet easy to use, file encryption library for .NET. 
It takes care of the nasty details so you don't have to.

SecretEngine supports both 32 bit and 64 bit. 
It's compiled for .NET Standard 2.1 which means is compatible for .NET 5 or later.

## Technical details

Under the hood SecretEngine uses Advanced Encryption Standard (AES) in CBC mode with 256bit keys. 
128 bit cryptographically random initialization vector (IV) is used.

For key derivation PBKDF2 function is used. Key derivation is configured to use 200000 iterations. 
Also, 256 bit cryptographically random salt is applied.

For authentication HMAC with SHA256 is used. SecretEngine uses encrypt-then-HMAC which 
is the preferred way for data authentication. This means that data is first encrypted 
and then HMACSHA256 is computed for the encrypted data. Verification of the HMAC is done 
in timing safe manner before attempting to decrypt the data.

