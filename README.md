<div align="center">

<img src="https://img.shields.io/badge/.NET-Backend-blue?style=for-the-badge&logo=dotnet" alt="Typing SVG" />

<br/>

[![TCP](https://img.shields.io/badge/Transport-TCP-blue?style=for-the-badge&logo=cisco&logoColor=white)](/)
[![WebSocket](https://img.shields.io/badge/Live-WebSocket-green?style=for-the-badge&logo=socketdotio&logoColor=white)](/)
[![Semaphore](https://img.shields.io/badge/Concurrency-Semaphore-orange?style=for-the-badge&logo=linux&logoColor=white)](/)
[![Priority Queue](https://img.shields.io/badge/Queue-Priority--Based-red?style=for-the-badge&logo=buffer&logoColor=white)](/)
[![Angular](https://img.shields.io/badge/Angular-Frontend-yellow?style=for-the-badge&logo=angular)](/)
<br/>

> **A real-time alert processing system** where multiple clients send alerts via TCP, the server processes them using a priority queue, and live results are streamed to the browser via WebSocket.

<br/>

---

</div>

## 📋 Table of Contents

- [✨ Features](#-features)
- [🏗️ System Architecture](#️-system-architecture)
- [⚙️ How It Works](#️-how-it-works)
- [🚦 Alert Priority System](#-alert-priority-system)
- [🖥️ Frontend Components](#️-frontend-components)
- [🔄 Flow Diagram](#-flow-diagram)
- [📁 Project Structure](#-project-structure)
- [🚀 Getting Started](#-getting-started)
- [🛠️ Tech Stack](#️-tech-stack)
- [📡 API & Protocol Reference](#-api--protocol-reference)

---

## ✨ Features

| Feature | Description |
|---|---|
| 🔌 **TCP Client Connections** | Clients connect to the server via raw TCP sockets to send alerts |
| 🔒 **Semaphore-Controlled Access** | Up to **4 clients** can be connected simultaneously using semaphores |
| 📊 **Priority Queue Processing** | Alerts are categorized and processed in **CRITICAL → HIGH → LOW** order |
| ⚡ **Live WebSocket Streaming** | Alerts are pushed instantly to the browser via built-in WebSocket |
| ⏱️ **3-Second Processing Interval** | Server processes one alert every 3 seconds, strictly priority-wise |
| 👁️ **60-Second Live Alert View** | Live alerts auto-disappear from the dashboard after 60 seconds |
| ✅ **Persistent Processed Alerts** | Completed alerts remain permanently on the processed alerts panel |

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT SIDE                              │
│                                                                 │
│   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│   │ Client 1 │  │ Client 2 │  │ Client 3 │  │ Client 4 │      │
│   └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘      │
│        │              │              │              │            │
└────────┼──────────────┼──────────────┼──────────────┼───────────┘
         │     TCP      │    TCP       │    TCP       │    TCP
         └──────────────┴──────────────┴──────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SERVER SIDE                              │
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │              SEMAPHORE GATE (Max: 4 clients)            │   │
│   └───────────────────────┬─────────────────────────────────┘   │
│                           │                                     │
│                           ▼                                     │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                   TCP LISTENER                          │   │
│   │              (Receives raw alert data)                  │   │
│   └───────────────────────┬─────────────────────────────────┘   │
│                           │                                     │
│                           ▼                                     │
│   ┌────────────────── CATEGORIZER ──────────────────────────┐   │
│   │                                                         │   │
│   │   ┌─────────────┐  ┌───────────┐  ┌─────────────┐      │   │
│   │   │  🔴 CRITICAL │  │ 🟡 HIGH  │  │  🟢 LOW     │      │   │
│   │   │   Priority  │  │ Priority  │  │  Priority   │      │   │
│   │   │    Queue    │  │   Queue   │  │    Queue    │      │   │
│   │   └──────┬──────┘  └─────┬─────┘  └──────┬──────┘      │   │
│   │          └───────────────┴────────────────┘             │   │
│   └──────────────────────────┬──────────────────────────────┘   │
│                              │ (processed every 3 seconds)      │
│                              ▼                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │              ALERT PROCESSOR                            │   │
│   │        (Status: PENDING → COMPLETED)                   │   │
│   └───────────────────────┬─────────────────────────────────┘   │
│                           │                                     │
│                           ▼                                     │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │              WEBSOCKET SERVER (Built-in)                │   │
│   └───────────────────────┬─────────────────────────────────┘   │
│                                                                 │
└───────────────────────────┼─────────────────────────────────────┘
                            │ WebSocket
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                       FRONTEND (Browser)                        │
│                                                                 │
│   ┌──────────────────────────┐  ┌──────────────────────────┐    │
│   │    📡 LIVE ALERTS        │  │   ✅ PROCESSED ALERTS    │    │
│   │                          │  │                          │    │
│   │  Shows alerts as they    │  │  Shows completed alerts  │    │
│   │  arrive in real-time     │  │  persistently            │    │
│   │                          │  │                          │    │
│   │  ⏳ Auto-disappears       │  │  ♾️ Stays permanently    │    │
│   │     after 60 seconds     │  │     on screen            │    │
│   └──────────────────────────┘  └──────────────────────────┘    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## ⚙️ How It Works

### 1️⃣ Client Connection via TCP
Clients connect to the server using a TCP socket and send alert payloads. The server uses a **semaphore** to enforce a maximum of **4 concurrent client connections**. Any additional clients are blocked until a slot becomes available.

```
Client → [TCP Connect] → Semaphore Gate → Server Handler
```

### 2️⃣ Alert Categorization
Once an alert is received, the server reads the alert's priority field and enqueues it into the appropriate priority queue:

- 🔴 `CRITICAL` → Highest priority queue
- 🟡 `HIGH`     → Medium priority queue
- 🟢 `LOW`      → Lowest priority queue

### 3️⃣ Priority-Based Processing
The processor runs on a **3-second interval**. On each tick, it dequeues **one alert** following strict priority order:

```
if CRITICAL queue not empty → process CRITICAL
else if HIGH queue not empty → process HIGH
else if LOW queue not empty  → process LOW
```

The alert's status is then updated from `PENDING` → `COMPLETED`.

### 4️⃣ WebSocket Broadcast
Every event — new alert received, alert processed — is broadcast to all connected browser clients via the built-in **WebSocket server**. The frontend reacts in real time.

---

## 🚦 Alert Priority System

```
┌────────────────────────────────────────────────────────────────┐
│                    PRIORITY PROCESSING ORDER                   │
│                                                                │
│   🔴 CRITICAL  ════════════════════════════════  First         │
│                  System down, data loss, outage                │
│                                                                │
│   🟡 HIGH      ════════════════════════          Second        │
│                  Performance degraded, warnings                │
│                                                                │
│   🟢 LOW       ════════════════                  Last          │
│                  Informational, minor issues                   │
│                                                                │
│   ─────────────────────────────────────────────────────────   │
│   ⏱️  One alert processed every 3 seconds                     │
│   ⬆️  Upper-priority queue fully drained before moving down   │
└────────────────────────────────────────────────────────────────┘
```

| Priority | Color Code | Use Case | Processing Order |
|----------|-----------|----------|-----------------|
| 🔴 CRITICAL | `#FF0000` | System failures, outages, data loss | 1st |
| 🟡 HIGH | `#FFA500` | Degraded performance, major warnings | 2nd |
| 🟢 LOW | `#00AA00` | Informational, minor issues, notices | 3rd |

> **Rule:** If even a single `CRITICAL` alert is in the queue, no `HIGH` or `LOW` alert will be processed until it is cleared.

---

## 🖥️ Frontend Components

### Component 1 — 📡 Live Alerts Panel

```
┌────────────────────────────────────────┐
│         📡  LIVE ALERTS                │
│                                        │
│  ┌──────────────────────────────────┐  │
│  │ 🔴 CRITICAL  |  Server Down      │  │
│  │  Client: 03  |  ⏳ 42s ago      │  │
│  └──────────────────────────────────┘  │
│  ┌──────────────────────────────────┐  │
│  │ 🟡 HIGH      |  Memory Spike     │  │
│  │  Client: 01  |  ⏳ 15s ago      │  │
│  └──────────────────────────────────┘  │
│                                        │
│  ✦ Alerts disappear after 60 seconds  │
└────────────────────────────────────────┘
```

- Displays **every alert the moment it arrives** at the server from any client
- Each alert card shows: **priority**, **message**, **client ID**, and **receiving time**
- Automatically **removed from view after 60 seconds** from the time of arrival
- Does **not** depend on processing status — shows raw incoming alerts

---

### Component 2 — ✅ Processed Alerts Panel

```
┌────────────────────────────────────────┐
│         ✅  PROCESSED ALERTS           │
│                                        │
│  ┌──────────────────────────────────┐  │
│  │ 🔴 CRITICAL  |  Server Down      │  │
│  │  Status: ✅ COMPLETED  |  03:42  │  │
│  └──────────────────────────────────┘  │
│  ┌──────────────────────────────────┐  │
│  │ 🟡 HIGH      |  Memory Spike     │  │
│  │  Status: ✅ COMPLETED  |  03:30  │  │
│  └──────────────────────────────────┘  │
│                                        │
│  ✦ Alerts here never disappear        │
└────────────────────────────────────────┘
```

- Displays only alerts whose status has changed to `COMPLETED` (i.e., processed by the server)
- Alerts are added **in the order they are processed** (priority-wise)
- These cards **never disappear** — they form a permanent log of handled alerts
- Useful as an **audit trail** of what was processed and when and of which **client**

---

## 🔄 Flow Diagram

```
Client Sends Alert
       │
       ▼
  TCP Connection
       │
  [Semaphore Check]
  Max 4 concurrent?
     /       \
   YES        NO → Wait / Reject
    │
    ▼
Receive Alert Payload
    │
    ▼
Categorize by Priority
    │
  ┌─┴──────────────────────┐
  ▼                        ▼                  ▼
🔴 CRITICAL Queue    🟡 HIGH Queue     🟢 LOW Queue
    │
    └─────────────────────────┐
                              ▼
                   [Every 3 Seconds]
                   Processor Ticks
                              │
                  ┌───────────┴───────────┐
                  │   Dequeue 1 Alert     │
                  │  (CRITICAL first,     │
                  │   then HIGH, LOW)     │
                  └───────────┬───────────┘
                              │
                  Status: PENDING → COMPLETED
                              │
                  ┌───────────┴───────────┐
                  │  Broadcast via        │
                  │  WebSocket            │
                  └───────────┬───────────┘
                              │
              ┌───────────────┴────────────────┐
              ▼                                ▼
     📡 Live Alerts Panel          ✅ Processed Alerts Panel
    (Shown, disappears 60s)       (Shown, stays permanently)
```

---

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| 🔌 Transport | **TCP Sockets** | Client-to-server alert delivery |
| 🔒 Concurrency | **SemaphoreSlim** | Limit to 4 simultaneous clients |
| 📊 Data Structure | **Priority Queue** | CRITICAL / HIGH / LOW ordering |
| 📡 Real-Time Push | **WebSocket (Built-in)** | Server-to-browser live streaming |
| 🖥️ Frontend | **Angular** | Live dashboard UI |
| ⏱️ Timer | **Interval (3s)** | Periodic alert processing |
