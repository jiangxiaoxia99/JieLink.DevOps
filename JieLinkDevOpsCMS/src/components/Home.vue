<template>
  <el-container class="home_container">
    <el-header>
      <div class="home_title">JieLink运维平台</div>
      <div class="home_userinfoContainer">
        <el-dropdown @command="handleCommand">
          <span class="el-dropdown-link home_userinfo">
            {{currentUserName}}
            <i class="el-icon-arrow-down el-icon--right home_userinfo"></i>
          </span>
          <el-dropdown-menu slot="dropdown">
            <el-dropdown-item command="sysMsg">系统消息</el-dropdown-item>
            <el-dropdown-item command="MyHome">个人主页</el-dropdown-item>
            <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
          </el-dropdown-menu>
        </el-dropdown>
      </div>
    </el-header>
    <el-container>
      <el-aside width="200px">
        <el-menu
          default-active="0"
          class="el-menu-vertical-demo"
          style="background-color: #ECECEC"
          router
        >
          <template v-for="(item,index) in this.$router.options.routes" v-if="!item.hidden">
            <el-submenu :index="index+''" v-if="item.children.length>1" :key="index">
              <template slot="title">
                <i :class="item.iconCls"></i>
                <span>{{item.name}}</span>
              </template>
              <el-menu-item
                v-for="child in item.children"
                v-if="!child.hidden||isAdmin"
                :index="child.path"
                :key="child.path"
              >{{child.name}}</el-menu-item>
            </el-submenu>
            <template v-else>
              <el-menu-item v-if="!item.children[0].hidden||isAdmin" :index="item.children[0].path">
                <i :class="item.children[0].iconCls"></i>
                <span slot="title">{{item.children[0].name}}</span>
              </el-menu-item>
            </template>
          </template>
        </el-menu>
      </el-aside>
      <el-container>
        <el-main>
          <el-breadcrumb separator-class="el-icon-arrow-right">
            <el-breadcrumb-item :to="{ path: '/home' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item v-text="this.$router.currentRoute.name"></el-breadcrumb-item>
          </el-breadcrumb>
          <keep-alive>
            <router-view v-if="this.$route.meta.keepAlive"></router-view>
          </keep-alive>
          <router-view v-if="!this.$route.meta.keepAlive"></router-view>
        </el-main>
        <el-footer>
          <a href="http://106.53.255.16:8090/" target="_blank">由JieLink+V2.*团队提供技术支持</a>
        </el-footer>
      </el-container>
    </el-container>
  </el-container>
</template>
<script>
import { getRequest } from "../utils/api";
export default {
  methods: {
    handleCommand(command) {
      var _this = this;
      if (command == "logout") {
        this.$confirm("注销登录吗?", "提示", {
          confirmButtonText: "确定",
          cancelButtonText: "取消",
          type: "warning"
        }).then(
          function() {
            getRequest("/logout");
            localStorage.removeItem("username");
            // 正式发布的时候 这段代码可以注释掉 由服务端控制跳转
            _this.$router.replace({ path: "/" });
          },
          function() {
            //取消
          }
        );
      }
    }
  },
  mounted: function() {

    if (localStorage.getItem("username") === "jielink") {
      this.isAdmin = false;//做了个假的权限控制
    } else {
      this.isAdmin = true;
    }

    console.log(this.isAdmin);

    var _this = this;
    getRequest("user/currentUserName").then(
      function(msg) {
        _this.currentUserName = msg.data;
      },
      function(msg) {
        _this.currentUserName = "游客";
      }
    );
  },
  data() {
    return {
      currentUserName: "",
      isAdmin: false
    };
  }
};
</script>
<style>
.home_container {
  height: 100%;
  position: absolute;
  top: 0px;
  left: 0px;
  width: 100%;
}

.el-header {
  background-color: #20a0ff;
  color: #333;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.el-aside {
  background-color: #ececec;
}

.el-main {
  background-color: #fff;
  color: #000;
  text-align: center;
}

.home_title {
  color: #fff;
  font-size: 22px;
  display: inline;
}

.home_userinfo {
  color: #fff;
  cursor: pointer;
}

.home_userinfoContainer {
  display: inline;
  margin-right: 20px;
}
</style>
