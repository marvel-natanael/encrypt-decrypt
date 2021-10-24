# encrypt-decrypt

## Overview
This is a simple chat app with hybrid encryption using AES and RSA algorithm
## Algorithm
1. Client generates public and private keys
2. Server obtain client's public key
3. Server creates a symmetric key & encrypts it using client's public key
4. Client obtain the symmetric key & decrypts with its private key
5. Server & client now uses symmetric key
## Result
![alt text](https://github.com/marvel-natanael/encrypt-decrypt/blob/main/Image%201.png)
