import { describe, expect, it } from "vitest";
import { getNextOrderStatus, orderWorkflow } from "./db";

describe("OrderFlow persistent order workflow", () => {
  it("permits exactly the defined next status at each non-terminal stage", () => {
    expect(getNextOrderStatus("created")).toBe("confirmed");
    expect(getNextOrderStatus("confirmed")).toBe("processing");
    expect(getNextOrderStatus("processing")).toBe("shipped");
    expect(getNextOrderStatus("shipped")).toBe("delivered");
    expect(getNextOrderStatus("delivered")).toBe("completed");
  });

  it("does not allow a completed order to advance further", () => {
    expect(getNextOrderStatus(orderWorkflow[orderWorkflow.length - 1])).toBeNull();
  });
});

