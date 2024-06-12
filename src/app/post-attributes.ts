export default interface PostAttributes {
  type: string;
  title: string;
  slug: string;
  description: string;
  date: Date | undefined;
}
