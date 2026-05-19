import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
} from "@/components/ui/sidebar"

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader />
      <SidebarContent>
        <SidebarGroup />
        asc
        <SidebarGroup />


        <SidebarGroup />
        ascA
        <SidebarGroup />

      </SidebarContent>

      <SidebarFooter />

    </Sidebar>
  )
}
