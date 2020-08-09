# Chatroom

A demo WPF(.NET Core 3.1) program to chat with friends in LAN.

Made for practicing programming skills.

Run `ipconfig` for server IP address.

## TODO

Textbox显示接受信息时有时会不换行。

问题已定位，Socket收发时可能会合并字节流。

自己写协议（用定长标记长度）或者用现成的（例如序列化）