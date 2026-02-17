<div align="center">

<img src="https://img.shields.io/badge/.NET-Backend-orange?style=for-the-badge&logo=dotnet" alt="Typing SVG" />

<br/>

[![TCP](https://img.shields.io/badge/Transport-TCP-blue?style=for-the-badge&logo=cisco&logoColor=white)](/)
[![WebSocket](https://img.shields.io/badge/Live-WebSocket-green?style=for-the-badge&logo=socketdotio&logoColor=white)](/)
[![Angular](https://img.shields.io/badge/Angular-Frontend-yellow?style=for-the-badge&logo=angular)](/)
<br/>

> **A real-time alert processing system** where multiple clients send alerts via TCP, the server stores them in three separate in-memory queues based on priority, and live results are streamed to the browser via WebSocket.

<br/>

---

</div>

## ✨ Features

| Feature | Description |
|---|---|
| 🔌 **TCP Client Connections** | Clients connect to the server via raw TCP sockets to send alerts |
| 📊 **In-Memory Queue Processing** | Alerts are stored in **three separate queues (CRITICAL, HIGH, LOW)** and processed in **CRITICAL → HIGH → LOW** order |
| ⚡ **Live WebSocket Streaming** | Alerts are pushed instantly to the browser via built-in WebSocket |
| ⏱️ **3-Second Processing Interval** | Server processes one alert every 3 seconds, strictly priority-wise |
| 👁️ **60-Second Live Alert View** | Live alerts auto-disappear from the dashboard after 60 seconds |

---

## ⚙️ How It Works

### 1️⃣ Client Connection via TCP
Clients connect to the server using a TCP socket and send alert payloads. The server uses a **semaphore** to enforce a maximum of **4 concurrent client connections**. Any additional clients are blocked until a slot becomes available.

```
Client → [TCP Connect] → Semaphore Gate → Server Handler
```

### 2️⃣ Alert Categorization into Separate Queues
Once an alert is received, the server reads the alert's priority field and stores it into one of three separate in-memory queues based on its priority:

- 🔴 `CRITICAL` → Highest priority queue
- 🟡 `HIGH`     → Medium priority queue
- 🟢 `LOW`      → Lowest priority queue

### 3️⃣ Queue-Based Priority Processing
The processor runs on a **3-second interval**. On each tick, it checks the three queues and dequeues **one alert** based on strict priority order:

```
if CRITICAL queue not empty → process CRITICAL
else if HIGH queue not empty → process HIGH
else if LOW queue not empty  → process LOW
```

The alert's status is then updated from `PENDING` → `COMPLETED`.

### 4️⃣ WebSocket Broadcast
Every event — new alert received, alert processed — is broadcast to all connected browser clients via the built-in **WebSocket server**. The frontend reacts in real time.

---

## 🖥️ Frontend Components

### Component 1 — 📡 Live Alerts Panel

- Displays **every alert the moment it arrives** at the server from any client
- Each alert card shows: **priority**, **message**, **client ID**, and **receiving time**
- Automatically **removed from view after 60 seconds** from the time of arrival
- Does **not** depend on processing status — shows raw incoming alerts

---

### Component 2 — ✅ Processed Alerts Panel

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
Categorize and Store in Separate Queue
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
| 📡 Real-Time Push | **WebSocket (Built-in)** | Server-to-browser live streaming |
| 🖥️ Frontend | **Angular** | Live dashboard UI |
