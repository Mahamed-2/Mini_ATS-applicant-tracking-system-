# Prompt 2 – AppShell & Dashboard Framework Specification

Reproduces the `code.html` layout structure:
1. FrameworkSpecBanner (Terminal icon + "● Store Sync Live" pill + title + Act-as Context chip + "Exit Scope" button)
2. Two-column row:
   - StoreInspectorCard: `useAppShellStore()` key/value rows + last latency pill + Toggle/Reset buttons
   - LayerBreakdownCard: RAIL SPEC, HEADER SPEC, VIEWPORT CANVAS tiles + status
3. RouteSimulatorGrid: 5 module cards (Dashboard, Jobs Manager, Candidates, Pipeline Board, System Admin) with real counts & launch buttons
4. Three-column row:
   - ToastSandboxCard
   - CandidateMicroCard live pattern
   - MultiTenantSwitcher radio selector
5. ConformanceNotesGrid: 4-column architecture specification
