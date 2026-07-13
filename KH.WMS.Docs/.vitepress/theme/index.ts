import DefaultTheme from 'vitepress/theme';
import { h } from 'vue';
import { useData } from 'vitepress';
import Home from './components/Home.vue';
import './style.css';

export default {
  extends: DefaultTheme,
  Layout: () => {
    const { frontmatter } = useData();
    return frontmatter.value.layout === 'home' ? h(Home) : h(DefaultTheme.Layout);
  }
};
