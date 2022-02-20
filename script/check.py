#!/usr/bin/env python

import sys
import re
from types import NoneType


def check_rtt(rttStr):
    result = re.search(
        r'Average round trip delay (.*) ms; min = (.*) ms, max = (.*) ms', rttStr)
    if type(result) is NoneType:
        exit(1)

    avg = float(result.group(1))
    min = float(result.group(2))
    max = float(result.group(3))

    print(f'{avg} {min} {max}')

    return


def check_jitter(jitterStr):
    result = re.search(
        r'Average jitter (.*) ms; min = (.*) ms, max = (.*) ms', jitterStr)
    if type(result) is NoneType:
        exit(1)

    avg = float(result.group(1))
    min = float(result.group(2))
    max = float(result.group(3))

    print(f'{avg} {min} {max}')

    return


def check_byte(byteStr):
    result = re.search(
        r'start_mclient: tot_send_bytes ~ (.*), tot_recv_bytes ~ (.*)', byteStr)
    if type(result) is NoneType:
        exit(1)

    total_send = int(result.group(1))
    total_recv = int(result.group(2))

    print(f'send: {total_send} byte\nrecv: {total_recv} byte')
    return


def check_lost_packets(lost_packetsStr):
    result = re.search(
        r'Total lost packets (.*) \((.*)%\), total send dropped (.*) \((.*)%\)', lost_packetsStr)

    if type(result) is NoneType:
        exit(1)

    lost_packets = int(result.group(1))
    lost_packets_ratio = float(result.group(2))
    dropped_packets = int(result.group(3))
    dropped_packets_ratio = float(result.group(4))

    if lost_packets > 0:
        exit(1)
    if dropped_packets > 0:
        exit(1)

    print(f'lost packets: {lost_packets} ratio: {lost_packets_ratio}')
    print(f'dropped packets: {dropped_packets} ratio: {dropped_packets_ratio}')
    return


def main():
    rtt = ''
    jitter = ''
    byte = ''
    lost_packets = ''
    for line in sys.stdin:
        if 'Average round trip delay' in line:
            rtt = line
        if 'Average jitter' in line:
            jitter = line
        if 'start_mclient: tot_send_bytes' in line:
            byte = line
        if 'Total lost packets' in line:
            lost_packets = line

    check_rtt(rtt)
    check_jitter(jitter)
    check_byte(byte)
    check_lost_packets(lost_packets)
    print('RTT: '+rtt, end="")
    print('Jitter: '+jitter, end="")
    print('Byte: '+byte, end="")
    print('Lost Packets: '+lost_packets, end="")


if __name__ == "__main__":
    main()
