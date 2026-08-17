import { describe, expect, it } from "vitest";
import { appRouter } from "./routers";
import { NOT_ADMIN_ERR_MSG } from "../shared/const";
import type { TrpcContext } from "./_core/context";

function userContext(): TrpcContext {
  return {
    user: {
      id: 42,
      openId: "non-admin-user",
      email: "user@example.com",
      name: "Standard User",
      loginMethod: "manus",
      role: "user",
      createdAt: new Date(),
      updatedAt: new Date(),
      lastSignedIn: new Date(),
    },
    req: { protocol: "https", headers: {} } as TrpcContext["req"],
    res: { clearCookie: () => undefined } as TrpcContext["res"],
  };
}

describe("OrderFlow persistent mutation authorization", () => {
  it("rejects a standard user before an inventory adjustment reaches the database", async () => {
    const caller = appRouter.createCaller(userContext());
    await expect(caller.orderflow.adjustInventory({ productId: 1, quantityDelta: 1 })).rejects.toMatchObject({ message: NOT_ADMIN_ERR_MSG });
  });

  it("rejects a standard user before an order workflow transition reaches the database", async () => {
    const caller = appRouter.createCaller(userContext());
    await expect(caller.orderflow.advanceOrder({ orderId: 1 })).rejects.toMatchObject({ message: NOT_ADMIN_ERR_MSG });
  });
});
