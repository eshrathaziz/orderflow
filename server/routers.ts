import { z } from "zod";
import { COOKIE_NAME } from "@shared/const";
import {
  adjustPersistentInventory,
  advancePersistentOrder,
  createPersistentOrder,
  createPersistentRequest,
  getWorkspace,
} from "./db";
import { getSessionCookieOptions } from "./_core/cookies";
import { systemRouter } from "./_core/systemRouter";
import { adminProcedure, protectedProcedure, publicProcedure, router } from "./_core/trpc";

const lineItem = z.object({ productId: z.number().int().positive(), quantity: z.number().int().positive() });

export const appRouter = router({
  system: systemRouter,
  auth: router({
    me: publicProcedure.query(({ ctx }) => ctx.user),
    logout: publicProcedure.mutation(({ ctx }) => {
      ctx.res.clearCookie(COOKIE_NAME, { ...getSessionCookieOptions(ctx.req), maxAge: -1 });
      return { success: true } as const;
    }),
  }),
  orderflow: router({
    workspace: protectedProcedure.query(({ ctx }) => getWorkspace(ctx.user.id)),
    createOrder: protectedProcedure
      .input(z.object({ customerId: z.number().int().positive(), items: z.array(lineItem).min(1).max(25), taxRate: z.number().min(0).max(0.25), shippingAddress: z.string().max(2000).optional(), notes: z.string().max(2000).optional() }))
      .mutation(({ ctx, input }) => createPersistentOrder(ctx.user.id, input)),
    adjustInventory: adminProcedure
      .input(z.object({ productId: z.number().int().positive(), quantityDelta: z.number().int().refine(value => value !== 0, "Quantity must change."), notes: z.string().max(1000).optional() }))
      .mutation(({ ctx, input }) => adjustPersistentInventory(ctx.user.id, input)),
    advanceOrder: adminProcedure
      .input(z.object({ orderId: z.number().int().positive() }))
      .mutation(({ ctx, input }) => advancePersistentOrder(ctx.user.id, input.orderId)),
    createRequest: protectedProcedure
      .input(z.object({ customerId: z.number().int().positive(), orderId: z.number().int().positive().optional(), type: z.enum(["change", "cancellation", "delivery", "billing", "other"]), priority: z.enum(["low", "medium", "high", "urgent"]), description: z.string().trim().min(5).max(4000) }))
      .mutation(({ ctx, input }) => createPersistentRequest(ctx.user.id, { ...input, authorName: ctx.user.name || ctx.user.email || "OrderFlow user" })),
  }),
});

export type AppRouter = typeof appRouter;
