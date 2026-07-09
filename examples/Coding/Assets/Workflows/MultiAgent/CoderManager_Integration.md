### Phase 4: Integration & System Verification (Closed-Loop Test & Repair)
- **Delegate Integration**: Once all implementation tasks are completed, delegate a comprehensive closed-loop integration task to the `IntegrationEngineer`.
- **Scope of Integration Prompting**: You must instruct the `IntegrationEngineer` to execute its multi-phase runtime validation loop strictly according to the System Integration & Runtime Contract (SIRC):
  1. **Document Ingestion**: Read `docs/tech_spec.md` and `docs/integration_contract.md` to establish the baseline contract matrix.
  2. **Tooling Verification & Synthesis**: Check for missing test managers, harnesses, or simulation tools required by the SIRC, and build/complete them first if absent.
  3. **Test Case Implementation**: Write the concrete integration test case code or automated scripts designed to drive the simulation input hooks.
  4. **Runtime Testing & Diagnostics**: Execute the build, inject virtual events via the EventBus, track the `TraceID` causal chains, and monitor live logs. If an assertion fails or times out, invoke `DumpState()` to capture the frozen state snapshot.
- **Handle Integration Failures (The Repair Loop)**: If the `IntegrationEngineer` encounters a contract violation, it will halt and yield a structured JSON Fix Ticket. You MUST parse this ticket, identify the responsible Coder Agent (e.g., `ComponentCoder` for UI/HUD mismatches, `LogicCoder` for core bus deadlocks), re-delegate the specific bug-fix task, and then re-awaken the `IntegrationEngineer` to run the entire verification cycle from Phase 0 again. Repeat this closed loop until the build passes all integration test cases.




- **CRITICAL CONSTRAINT: This workflow strictly prohibits the execution of any isolated unit tests. Testing is strictly focused on end-to-end integration scenario execution via runtime simulation hooks.**