# Speed Cube Timer - BDD Specification

## User Stories

### Story 1: Basic Timing
**As a** speedcuber  
**I want** to start and stop a timer with the SPACE key  
**So that** I can measure my solve times hands-free

**Acceptance Criteria:**
- Hold SPACE for 0.5+ seconds → Timer starts
- Press SPACE (any duration) when running → Timer stops immediately
- Time is automatically recorded and displayed

### Story 2: Performance Tracking
**As a** speedcuber  
**I want** to see statistics of my solves  
**So that** I can track my improvement

**Acceptance Criteria:**
- Best time shows the fastest solve
- Worst time shows the slowest solve
- Average time shows the mean of all solves
- Statistics update after each solve

### Story 3: Visual Trends
**As a** speedcuber  
**I want** to see a plot of my solve times over the session  
**So that** I can visualize my performance trends

**Acceptance Criteria:**
- Plot displays all recorded times as a line chart
- Points are selectable by clicking
- Selected points are highlighted in orange
- Plot updates after each new solve

### Story 4: Session Management
**As a** speedcuber  
**I want** to delete individual solves or reset my entire session  
**So that** I can manage my data

**Acceptance Criteria:**
- Delete button removes selected time
- R key resets entire session
- History and plot update after deletion/reset
- Statistics recalculate immediately

### Story 5: User Interface
**As a** speedcuber  
**I want** a dark-themed, responsive interface  
**So that** I can use it comfortably in any lighting

**Acceptance Criteria:**
- Black background with white text
- Large, centered timer display
- Sidebar with history and trends
- Responsive layout at 1200x800 resolution
