clc
clear all
tcpipClient = tcpip('127.0.0.1',55001,'NetworkRole','Client');
set(tcpipClient,'Timeout',30);
fopen(tcpipClient);
a='yah!! we could make it';
fwrite(tcpipClient,a);
fclose(tcpipClient);