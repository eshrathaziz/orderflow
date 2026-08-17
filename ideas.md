# OrderFlow — Design Direction

## Three Possible Directions

### 1. Operations Ledger
**Very Brief Intro:** A calm, high-accountability operational console inspired by logistics control rooms and well-kept paper ledgers. Dense information is organized through strong typographic hierarchy, quiet surfaces, and targeted cobalt signals.

**Probability:** 0.037

### 2. Warehouse Signal
**Very Brief Intro:** A technical dark-mode workspace that treats statuses, stock risk, and approval states as luminous operational signals. The emotional intent is alertness and real-time command.

**Probability:** 0.081

### 3. Supply Chain Atelier
**Very Brief Intro:** A warm, editorial interpretation of enterprise software that combines refined serif labels with tactile material tones. It is intended to make routine operations feel considered and human.

**Probability:** 0.024

## Selected Direction: Operations Ledger

### Design Movement
**Contemporary Swiss information design** translated for enterprise operations, with the restraint of aviation wayfinding and the practical clarity of a logistics control center.

### Core Principles
1. **Operational clarity before decoration:** every visual decision should accelerate scanning, action, or confidence.
2. **Structured density:** information-rich pages remain calm through alignment, generous row rhythm, and deliberate whitespace.
3. **Status has a language:** color, label, and icon consistently express attention, progress, completion, or risk.
4. **Authority through restraint:** surfaces are quiet and typography is decisive; visual emphasis is earned rather than sprayed everywhere.

### Color Philosophy
The interface is built around an **ink-blue foundation** that conveys trust and order, warm off-white work surfaces that avoid clinical sterility, and a single vivid **Signal Cobalt** accent for primary action and in-progress operational states. Sea-green denotes successful completion, amber represents pending attention, and vermilion is reserved for shortage and destructive actions. The color system should feel like a physical control room: mostly neutral instrumentation, with signals only where operators need to look.

### Layout Paradigm
A persistent dark navigation rail anchors the workspace. The content area is not a centered website canvas: it behaves as a **ledger field**, beginning with a generous operational header, then flowing into asymmetric metrics, analysis cards, and horizontally scrollable work queues. Major content blocks align to a shared left edge, while secondary context and contextual actions sit in a narrow right-hand zone.

### Signature Elements
1. **Ledger rule:** thin horizontal hairlines and column labels frame information like a contemporary operations journal.
2. **Signal dot:** a compact colored square/dot paired with status labels, reused in orders, inventory, and request priority.
3. **Cobalt action slab:** primary actions use a crisp blue rectangular field with modest corner radius and an offset shadow, communicating clear operator intent.

### Interaction Philosophy
Interactions should feel like operating reliable equipment. Hover states sharpen contrast and introduce a small positional shift; status changes receive immediate, explicit confirmation. Filters and work-queue controls respond predictably without visual theatrics. Destructive actions require a deliberate confirmation step.

### Animation
Use a 160–220ms custom ease-out transition for button, row, and filter states. Cards can lift by 1–2px on hover; table rows should use a soft background transition rather than movement. Page content may enter in short 40ms staggered groups, but the dashboard never blocks data behind elaborate motion. Respect `prefers-reduced-motion` by eliminating all non-essential entrance movement.

### Typography System
Use **Manrope** for all UI copy because its open forms support dense scanning, paired with **DM Mono** for order identifiers, counts, inventory quantities, table metadata, and technical labels. Headings use Manrope at 600–700 weight with compact tracking; micro-labels use DM Mono uppercase at 10–11px with increased letter spacing. Avoid generic web typography and avoid oversized marketing-style hero text.

### Brand Essence
**OrderFlow is the operational command layer for teams that need orders, inventory, and customer action to move in one accountable flow.**

Personality: **Precise, composed, accountable.**

### Brand Voice
Headlines should sound directive and observant; CTAs should name the action and never rely on empty enthusiasm. Microcopy should make consequences clear.

Example lines:

> “Keep every order moving.”

> “18 units below the reorder threshold — review stock.”

### Wordmark & Logo
The mark is an abstract **flow channel**: three interlocking, forward-moving bands that form a subtle “O” in negative space. It implies items moving from order intake through fulfillment without relying on a letterform. The wordmark uses a custom-spaced Manrope semibold treatment in all lower case, with the mark used alone at compact sizes.

### Signature Brand Color
**Signal Cobalt — #2563EB.** It is reserved for committed actions, current navigation, focused controls, and active workflow states.

## Build Reminder

Every component should reinforce Operations Ledger: ink-blue structure, warm working surfaces, ledger rules, controlled density, and Cobalt only for intentional operational signals. If a decision dilutes this philosophy, do not use it.

## Style Decisions

- The primary workspace framing always includes an ink-blue navigation rail as the structural anchor; a conventional top-navigation layout alone is not sufficient.
- Dispatch imagery is treated as operating evidence, with HUB context, active-unit counts, exception state, and a dark monitor overlay rather than generic decoration.
- Status color is semantic: Signal Cobalt `#2563EB` denotes current and in-progress work, sea-green denotes completion, amber denotes pending attention, and vermilion denotes shortage or destructive risk.
