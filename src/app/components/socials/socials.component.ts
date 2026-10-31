import { Component } from '@angular/core';

@Component({
    selector: 'app-socials',
    imports: [],
    templateUrl: './socials.component.html',
    styleUrl: './socials.component.css'
})
export class SocialsComponent {
  SocialItems: ISocialItem[] = [
    { Text: "GitHub", Href: "https://github.com/RiddleTime/Race-Element" },
    { Text: "Discord", Href: "https://discord.gg/26AAEW5mUq" },
    { Text: "X", Href: "https://x.com/Race_Element" },
    { Text: "Donate", Href: "/guide/sponsor" },
  ];
}

export interface ISocialItem {
  Text: string;
  Href: string;
}
